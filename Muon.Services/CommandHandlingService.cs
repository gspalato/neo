using Discord;
using Discord.WebSocket;
using Muon.Commands;
using Muon.Kernel.Structures;
using Qmmands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Muon.Services
{
	public interface ICommandHandlingService
	{}

	public class CommandHandlingService : ICommandHandlingService
	{
		private DiscordSocketClient _client;
		private ICommandService _commandService;
		private IDatabaseService _databaseService;

		private IServiceProvider _services;

		public CommandHandlingService(DiscordSocketClient client,
			ICommandService commandService, IDatabaseService databaseService,
			IServiceProvider services)
		{
			_commandService = commandService;
			_client = client;
			_databaseService = databaseService;
			_services = services;

			LinkEvents();
			AddTypeParsers();

			_commandService.AddModules(Assembly.GetExecutingAssembly());
		}

		private void LinkEvents()
		{
			_commandService.CommandExecutionFailed += async (args) =>
			{
				Console.WriteLine(args.Result.Reason, args.Result.Exception?.StackTrace, args.Result.Exception?.Message);

				await Task.Run(() => { });
			};
			_client.MessageReceived += (msg) => OnMessageReceivedAsync(msg);
		}

		private void AddTypeParsers()
		{
			_commandService.AddTypeParser(new TextChannelParser());
			_commandService.AddTypeParser(new SocketGuildUserParser());
		}

		private async Task OnMessageReceivedAsync(IMessage msg)
		{
			if (!(msg is SocketUserMessage))
				return;

			SocketGuild guild = (msg.Channel as SocketGuildChannel).Guild;

			GuildSettings settings = await _databaseService.GetGuildSettingsAsync(guild.Id);
			if (settings == null)
				settings = await _databaseService.CreateGuildSettingsAsync(guild.Id);

			if (!CommandUtilities.HasPrefix(msg.Content, settings.prefix, out string output))
				return;

			IResult result = await _commandService.ExecuteAsync(output, new MuonContext(msg, _services));
			if (result is FailedResult failedResult)
				await msg.Channel.SendMessageAsync(failedResult.Reason); // always gives "An exception occurred ..."
		}
	}
}
