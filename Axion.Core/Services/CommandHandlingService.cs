using Axion.Commands;
using Axion.Core.Structures.TypeParsers;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Axion.Core.Services
{
	public interface ICommandHandlingService
	{ }

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
				_loggingService.Error(args.Result.Reason, args.Result.Exception);
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

		private async Task OnMessageReceivedAsync(IMessage m)
		{
			if (!(m is IUserMessage msg))
				return;

			var guild = ((ITextChannel)msg.Channel).Guild;

			var settings = await _guildSettingsRepository.GetOrCreateForGuildAsync(guild.Id);

			if (!CommandUtilities.HasPrefix(msg.Content, settings.prefix, out var output))
				return;

			var result = await _commandService.ExecuteAsync(output, new AxionContext(msg, _services));

			switch (result)
			{
				default:
				case CommandNotFoundResult notFound:
					break;

				case TypeParseFailedResult typeParse:
					{
						var name = typeParse.Parameter.Name;
						var type = typeParse.Parameter.Type.Name;
						var given = typeParse.Value;

						await m.Channel.SendMessageAsync(embed: new EmbedBuilder()
							.WithTitle("⚠️ Huh?")
							.WithColor(Color.Orange)
							.WithDescription($"Wrong type given at `{name}`.\nExpected {type}, got\n{Format.Quote(given)}")
							.Build());
					}
					break;

				case ParameterChecksFailedResult parameterChecks:
					{
						// need 2 handle this xd
					}
					break;

				case OverloadsFailedResult overloads:
					{
						// need 2 handle this xd
					}
					break;
			}
		}
	}
}
