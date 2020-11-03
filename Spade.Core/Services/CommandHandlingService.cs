using Spade.Core.Commands;
using Spade.Core.Commands.ArgumentParsers;
using Spade.Core.Commands.TypeParsers;
using Spade.Core.Structures.Attributes;
using Spade.Database.Repositories;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spade.Core.Services
{
	public interface ICommandHandlingService
	{
		void Start();
	}

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
		}

		public void Start()
		{
			LinkEvents();

			AddArgumentParsers();
			AddTypeParsers();

			_commandService.AddModules(Assembly.GetExecutingAssembly());
		}

		private void LinkEvents()
		{
			_commandService.CommandExecutionFailed += async args =>
			{
				var builder = new StringBuilder();

				builder.Append($"{args.Result.Exception.Message}\n{args.Result.Exception.StackTrace}");

				if (args.Result.Exception.InnerException != null)
				{
					var inner = args.Result.Exception.InnerException;
					builder.Append($"\nINNER EXCEPTION | {inner.Message}\n{inner.StackTrace}");
				}

				_loggingService.Error(builder.ToString());

				await Task.Yield();
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

			var context = new SpadeContext(msg, me, _services);
			var result = await _commandService.ExecuteAsync(output, context);

			switch (result)
			{
				case ArgumentParseFailedResult argParse:
					{
						await SendErrorResultEmbedAsync(msg, argParse.Reason);
					}
					break;

				case TypeParseFailedResult typeParse:
					{
						var name = typeParse.Parameter.Name;
						var type = typeParse.Parameter.Type.Name;
						var given = Format.Quote(Format.Sanitize(typeParse.Value));

						await SendErrorResultEmbedAsync(msg,
								$"Wrong type given at `{name}`.\nExpected {type}, got\n{given}");
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
