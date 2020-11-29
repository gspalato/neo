using Discord;
using Discord.WebSocket;
using Qmmands;
using Oculus.Common.Structures;
using Oculus.Core.Commands;
using Oculus.Core.Commands.ArgumentParsers;
using Oculus.Core.Commands.TypeParsers;
using Oculus.Core.Structures.Attributes;
using Oculus.Core.Structures.Exceptions;
using Oculus.Database.Repositories;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oculus.Core.Services
{
	public interface ICommandHandlingService
	{
		void Start();

		Task HandleCommandResult(IResult result, IUserMessage msg, OculusContext context = null);
	}

	public class CommandHandlingService : ServiceBase, ICommandHandlingService
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
				var context = args.Context as OculusContext;
				await HandleCommandResult(args.Result, context.Message, context);
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
			_commandService.AddTypeParser(TimeSpanTypeParser.Instance);
			_commandService.AddTypeParser(UserParser.Instance);
		}

		private async Task OnMessageReceivedAsync(IMessage m)
		{
			if (m is not IUserMessage msg)
				return;

			if (msg.Channel is not ITextChannel textChannel)
				return;

			var me = await textChannel.Guild.GetCurrentUserAsync();
			if (!me.GetPermissions(textChannel).SendMessages)
				return;

			var settings = await _guildSettingsRepository.GetOrCreateForGuildAsync(textChannel.GuildId);

			if (!CommandUtilities.HasPrefix(msg.Content, settings.Prefix, out var output))
				return;

			var context = new OculusContext(msg, me, _services);
			var result = await _commandService.ExecuteAsync(output, context);

			_ = HandleCommandResult(result, msg);
		}

		public async Task HandleCommandResult(IResult result, IUserMessage msg, OculusContext context = null)
		{
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

				case ChecksFailedResult checksFailed:
					{
						var sb = new StringBuilder();
						sb.AppendLine("Some checks have failed before executing the command:");

						foreach (var (check, res) in checksFailed.FailedChecks)
							sb.AppendLine($"> {res.Reason}");

						await SendErrorResultEmbedAsync(msg, sb.ToString());
					}
					break;

				case OverloadsFailedResult overloads:
					{
						await SendErrorResultEmbedAsync(msg, overloads.Reason);
					}
					break;

				case ExecutionFailedResult execution:
					{
						if (context is null)
							return;

						var exception = execution.Exception;

						if (execution.Exception is UserFriendlyCommandError)
						{
							await SendErrorResultEmbedAsync(msg, execution.Exception.Message);
							return;
						}

						var app = await context.Client.GetApplicationInfoAsync();
						var owner = app.Owner;

						var description = new StringBuilder();
						description.AppendLine(exception.Message);
						description.AppendLine(Format.Code(exception.StackTrace, ""));

						if (exception.InnerException is not null)
						{
							description.AppendLine(exception.InnerException.Message);
							description.AppendLine(Format.Code(exception.InnerException.StackTrace, ""));
						}

						var errorEmbed = new EmbedBuilder()
							.WithTitle("⚠️ Error")
							.WithColor(Color.Red)
							.WithDescription(description.ToString())
							.WithFooter($"{context.Guild.Name} at #{context.Channel.Name}")
							.WithCurrentTimestamp();

						await owner.SendMessageAsync(embed: errorEmbed.Build());
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
