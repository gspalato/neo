using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Muon.Commands;
using Muon.Kernel.Structures.TypeParsers;
using Qmmands;

namespace Muon.Services
{
	public interface ICommandHandlingService
	{}

	public class CommandHandlingService : ICommandHandlingService
	{
		private readonly DiscordSocketClient _client;
		private readonly ICommandService _commandService;
		private readonly ILoggingService _loggingService;

		private readonly IGuildSettingsRepository _guildSettingsRepository;

		private readonly IServiceProvider _services;

		public CommandHandlingService(DiscordSocketClient client,
			ICommandService commandService, ILoggingService loggingService,
			IGuildSettingsRepository guildSettingsRepository, IServiceProvider services)
		{
			_commandService = commandService;
			_client = client;
			_guildSettingsRepository = guildSettingsRepository;
			_loggingService = loggingService;
			_services = services;

			LinkEvents();
			AddTypeParsers();

			_commandService.AddModules(Assembly.GetExecutingAssembly());
		}

		private void LinkEvents()
		{
			_commandService.CommandExecutionFailed += async (args) =>
			{
				_loggingService.Error("", args.Result.Exception, args.Result.Reason);
				await Task.Run(() => { });
			};

			_client.MessageReceived += OnMessageReceivedAsync;
		}

		private void AddTypeParsers()
		{
			_commandService.AddTypeParser(new GuildUserParser());
			_commandService.AddTypeParser(new MessageParser());
			_commandService.AddTypeParser(new TextChannelParser());
			_commandService.AddTypeParser(new UserParser());
		}

		private async Task OnMessageReceivedAsync(IMessage msg)
		{
			if (!(msg is IUserMessage))
				return;

			var guild = ((IGuildChannel)msg.Channel).Guild;

			var settings = await _guildSettingsRepository.GetForGuildAsync(guild.Id)
			               ?? await _guildSettingsRepository.CreateForGuildAsync(guild.Id);

			if (!CommandUtilities.HasPrefix(msg.Content, settings.prefix, out var output))
				return;

			var result = await _commandService.ExecuteAsync(output, new MuonContext(msg, _services));
			if (result is FailedResult failedResult)
				await msg.Channel.SendMessageAsync(failedResult.Reason);
		}
	}
}
