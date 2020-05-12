using Axion.Core.Commands;
using Axion.Core.Commands.ArgumentParsers;
using Axion.Core.Commands.TypeParsers;
using Axion.Core.Database;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Axion.Core.Commands.Modules;
using Axion.Core.Structures.Attributes;

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
			_commandService.CommandExecutionFailed += async args =>
			{
				_loggingService.Error($"{args.Result.Exception.Message}\n{args.Result.Exception.StackTrace}");
                if (args.Result.Exception.InnerException != null)
                {
                    var inner = args.Result.Exception.InnerException;
                    _loggingService.Error($"INNER EXCEPTION | {inner.Message}\n{inner.StackTrace}");
				}
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
			_commandService.AddTypeParser(ModuleTypeParser.Instance);
			_commandService.AddTypeParser(TextChannelParser.Instance);
			_commandService.AddTypeParser(UserParser.Instance);
		}

		private async Task OnMessageReceivedAsync(IMessage m)
		{
			if (!(m is IUserMessage msg))
				return;

			if (!(msg.Channel is ITextChannel textChannel))
				return;

            var me = await textChannel.Guild.GetCurrentUserAsync();
            if (!me.GetPermissions(textChannel).SendMessages)
                return;

			var settings = await _guildSettingsRepository.GetOrCreateForGuildAsync(textChannel.GuildId);

			if (!CommandUtilities.HasPrefix(msg.Content, settings.Prefix, out var output))
				return;

            var context = new AxionContext(msg, me, _services);
			var result = await _commandService.ExecuteAsync(output, context);

            switch (result)
            {
				default:
					Console.WriteLine($"Result Type: {result.GetType().Name}");
                    break;

                case ArgumentParseFailedResult argParse:
                    {
                        await SendErrorResultEmbedAsync(msg, argParse.Reason);
                    }
                    break;

                case TypeParseFailedResult typeParse:
					{
						var name = typeParse.Parameter.Name;
						var type = typeParse.Parameter.Type.Name;
						var given = typeParse.Value;

						await SendErrorResultEmbedAsync(msg,
								$"Wrong type given at `{name}`.\nExpected {type}, got\n{Format.Quote(Format.Sanitize(given))}");
					}
					break;

				case ParameterChecksFailedResult parameterChecks:
                    {
                        var cmd = parameterChecks.Parameter.Command;
                        var shouldSupress = cmd.Attributes.Any(a => a is SupressPermissionErrorsAttribute);

                        if (shouldSupress)
                            return;

						var (_, parameterResult) = parameterChecks.FailedChecks.First();
						var reason = parameterResult.Reason;

						await SendErrorResultEmbedAsync(msg, reason);
                    }
					break;

				case OverloadsFailedResult overloads:
                    {
                        await SendErrorResultEmbedAsync(msg, overloads.Reason);
                    }
					break;
			}
		}

        private async Task SendErrorResultEmbedAsync(IMessage msg, string reason)
        {
            var footer = new EmbedFooterBuilder()
                .WithText(msg.Author.Username)
                .WithIconUrl(msg.Author.GetAvatarUrl());

            await msg.Channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("⚠️ Huh?")
                .WithColor(Color.Orange)
                .WithDescription(reason)
                .WithFooter(footer)
                .WithCurrentTimestamp()
                .Build());
		}
	}
}
