using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Qmmands;

using Arpa.Entities;
using Arpa.Entities.Results;
using Arpa.Structures;

namespace Arpa.Services
{
	public class CommandHandlerService : ICommandHandlerService
	{
		private readonly DiscordClient client;
		private readonly IServiceProvider services;

		private readonly CommandService commandService;

		public string prefix;

		public CommandHandlerService(
			DiscordClient client,
			CommandService commandService,
			IServiceProvider services
		)
		{
			this.services = services;
			this.client = client;

			this.commandService = commandService;
		}

		public void InstallCommandsAsync(string prefix)
		{
			this.client.MessageCreated += HandleCommandAsync;
			this.prefix = prefix;

			this.commandService.AddModules(Assembly.GetEntryAssembly());
		}

		private async Task HandleCommandAsync(MessageCreateEventArgs args)
		{
			DiscordMessage msg = args.Message;

			if (msg.Author.IsBot)
				return;

			if (!CommandUtilities.HasPrefix(msg.Content, this.prefix, out string output))
				return;

			Console.WriteLine($"content: {msg.Content}, out: {output}, guild: {msg.Channel.Guild.Name}");

			ArpaCommandContext ctx = new ArpaCommandContext(msg, services);

			IResult result = await commandService.ExecuteAsync(output, ctx);
			if (result is ArpaFailedResult failed)
				await this.HandleCommandFailAsync(msg, failed);
			else if (result == null)
				Console.WriteLine("null >:(");
		}

		public async Task HandleCommandFailAsync(DiscordMessage msg, ArpaFailedResult result)
		{
			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
				.WithTitle("⚠️ Error")
				.WithDescription(result.Reason)
				.WithFooter(msg.Author.Username, msg.Author.AvatarUrl)
				.WithTimestamp(DateTime.UtcNow);

			await msg.Channel.SendMessageAsync(embed: builder.Build());
		}
	}
}