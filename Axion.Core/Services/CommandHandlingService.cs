using Axion.Commands;
using Axion.Commands.ArgumentParsers;
using Axion.Commands.TypeParsers;
using Axion.Core.Repositories;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
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

			AddArgumentParsers();
			AddTypeParsers();

			_commandService.AddModules(Assembly.GetExecutingAssembly());
		}

		private void LinkEvents()
		{
			_commandService.CommandExecutionFailed += async (args) =>
			{
				_loggingService.Error(args.Result.Reason, args.Result.Exception);
				await Task.Delay(0);
			};

			_client.MessageReceived += OnMessageReceivedAsync;
		}

		private void AddArgumentParsers()
		{
			_commandService.AddArgumentParser(UnixArgumentParser.Instance);
		}

		private void AddTypeParsers()
		{
			_commandService.AddTypeParser(CommandTypeParser.Instance);
			_commandService.AddTypeParser(GuildUserParser.Instance);
			_commandService.AddTypeParser(MessageParser.Instance);
			_commandService.AddTypeParser(TextChannelParser.Instance);
			_commandService.AddTypeParser(UserParser.Instance);
		}

		private async Task OnMessageReceivedAsync(IMessage m)
		{
			if (!(m is IUserMessage msg))
				return;

			if (!(msg.Channel is ITextChannel textChannel))
				return;

			var settings = await _guildSettingsRepository.GetOrCreateForGuildAsync(textChannel.GuildId);

			if (!CommandUtilities.HasPrefix(msg.Content, settings.Prefix, out var output))
				return;

			var result = await _commandService.ExecuteAsync(output, new AxionContext(msg, _services));

			switch (result)
			{
				default:
				case CommandNotFoundResult _:
					break;

				case TypeParseFailedResult typeParse:
					{
						var name = typeParse.Parameter.Name;
						var type = typeParse.Parameter.Type.Name;
						var given = typeParse.Value;

						var footer = new EmbedFooterBuilder()
							.WithText(m.Author.Username)
							.WithIconUrl(m.Author.GetAvatarUrl());

						await m.Channel.SendMessageAsync(embed: new EmbedBuilder()
							.WithTitle("⚠️ Huh?")
							.WithColor(Color.Orange)
							.WithDescription(
								$"Wrong type given at `{name}`.\nExpected {type}, got\n{Format.Quote(Format.Sanitize(given))}")
							.WithFooter(footer)
							.WithCurrentTimestamp()
							.Build());
					}
					break;

				case ParameterChecksFailedResult parameterChecks:
					{
						var failedCheck = parameterChecks.FailedChecks.First();
						var reason = failedCheck.Result.Reason;

						var footer = new EmbedFooterBuilder()
							.WithText(m.Author.Username)
							.WithIconUrl(m.Author.GetAvatarUrl());

						await m.Channel.SendMessageAsync(embed: new EmbedBuilder()
							.WithTitle("⚠️ Huh?")
							.WithColor(Color.Orange)
							.WithDescription(reason)
							.WithFooter(footer)
							.WithCurrentTimestamp()
							.Build());
					}
					break;

				case OverloadsFailedResult overloads:
					{
						var footer = new EmbedFooterBuilder()
							.WithText(m.Author.Username)
							.WithIconUrl(m.Author.GetAvatarUrl());

						await m.Channel.SendMessageAsync(embed: new EmbedBuilder()
							.WithTitle("⚠️ Huh?")
							.WithColor(Color.Orange)
							.WithDescription(overloads.Reason)
							.WithFooter(footer)
							.WithCurrentTimestamp()
							.Build());
					}
					break;
			}
		}
	}
}
