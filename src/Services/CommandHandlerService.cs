using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;

using Arpa.Entities;
using Arpa.Structures;

namespace Arpa.Services
{
	public class CommandHandlerService : ICommandHandlerService
	{
		private readonly DiscordSocketClient client;
		private readonly IServiceProvider services;

		public string prefix;

		public CommandHandlerService(
			DiscordSocketClient client,
			IServiceProvider services
		)
		{
			this.services = services;
			this.client = client;
		}

		public Task InstallCommandsAsync(string prefix)
		{
			this.client.MessageReceived += HandleCommandAsync;
			this.prefix = prefix;

			try
			{
				services.GetRequiredService<CommandService>().AddModules();
			}
			catch (Exception e)
			{
				services.GetRequiredService<LoggingService>().LogAsync(e.ToString());
			}

			return Task.CompletedTask;
		}

		private async Task HandleCommandAsync(SocketMessage raw)
		{
			if (!(raw is SocketUserMessage msg))
				return;

			if (msg.Source != MessageSource.User)
				return;

			if (!(msg.Content.Substring(0, this.prefix.Length).Equals(this.prefix))
				|| msg.Content.Length <= this.prefix.Length)
				return;

			CommandContext ctx = new CommandContext(this.client, msg);

			await Task.Run(() => this.services.GetRequiredService<CommandService>().Execute(ctx))
				.ConfigureAwait(false);

			/*ICommandContext context = new SocketCommandContext(this.client, msg);

			await this.commands.ExecuteAsync(
				context: context,
				argPos: argPos,
				services: this.services
			);*/
		}

		public List<string> ParseMessage(string args, ParserMode type = ParserMode.Default)
		{
			List<string> parsedArguments = new List<string>();

			string currentArg = "";

			bool isQuotedArgument = false;

			for (int i = 0; i < args.Length; i++)
			{
				char character = args[i];

				if (i == (args.Length - 1) && character != '"')
				{
					currentArg += character;
					parsedArguments.Add(currentArg);
					break;
				}
				else if (i == (args.Length - 1) && character == '"')
				{
					parsedArguments.Add(currentArg);
					break;
				}

				if (isQuotedArgument)
				{
					currentArg += character;
					continue;
				}

				if (type == ParserMode.Default && char.IsWhiteSpace(character))
				{
					parsedArguments.Add(currentArg);
					currentArg = "";
					continue;
				}

				if (type == ParserMode.Default && character == '"')
				{
					if (isQuotedArgument)
					{
						parsedArguments.Add(currentArg);
						currentArg = "";
						isQuotedArgument = false;
						continue;
					}

					isQuotedArgument = true;
					continue;
				}

				currentArg += character;
			}

			return parsedArguments;
		}
	}
}