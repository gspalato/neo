using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Arpa.Entities;
using Arpa.Structures;

namespace Arpa.Services
{
	public interface ICommandHandlerService
	{
		Task InstallCommandsAsync(string prefix);
	}

	public class CommandHandlerService : ICommandHandlerService
	{
		private readonly DiscordSocketClient client;
		private readonly IServiceProvider services;
		private readonly CommandService commands;

		public string prefix;

		public CommandHandlerService(
			DiscordSocketClient client,
			CommandService commands,
			IServiceProvider services
		)
		{
			this.commands = commands;
			this.services = services;
			this.client = client;
		}

		public async Task InstallCommandsAsync(string prefix)
		{
			this.client.MessageReceived += HandleCommandAsync;
			this.prefix = prefix;

			await this.commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
												services: this.services);
		}

		private async Task HandleCommandAsync(SocketMessage raw)
		{
			if (!(raw is SocketUserMessage msg))
				return;

			if (msg.Source != MessageSource.User)
				return;

			int argPos = 0;

			if (!msg.HasStringPrefix(this.prefix, ref argPos)
				|| msg.Content.Length <= this.prefix.Length)
				return;

			_CommandContext ctx = new _CommandContext(this.client, msg);

			this.services.GetRequiredService<_CommandService>().Execute(ctx);

			await Task.Run(() => { }).ConfigureAwait(false);

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