using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using Muon;
using Muon.Core.Structures;
using Muon.Services;

namespace Muon.Services
{
	public class CommandService
	{
		private readonly DiscordClient client;
		private readonly CommandsNextExtension commands;
		public InteractivityExtension interactivity;

		private IServiceProvider services;

		public CommandService(
			DiscordClient client,
			IServiceProvider services)
		{
			this.client = client;
			this.services = services;

			this.commands = client.UseCommandsNext(new CommandsNextConfiguration
			{
				PrefixResolver = this.ParsePrefix,
				EnableDefaultHelp = false,
				EnableDms = false,
				Services = services,
			});

			this.interactivity = client.UseInteractivity(new InteractivityConfiguration { });
		}

		public void InstallCommandsAsync()
		{
			try
			{
				this.commands.RegisterCommands(Assembly.GetEntryAssembly());
				this.commands.CommandErrored += this.HandleCommandError;
			}
			catch { }
		}

		private async Task HandleCommandError(CommandErrorEventArgs e)
		{
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
				.WithTitle("Error")
				.WithColor(new DiscordColor(0xFFA500))
				.WithDescription(e.Exception.Message)
				.WithTimestamp(e.Context.Message.Timestamp);

			await e.Context.RespondAsync(embed: embed.Build());
		}

		private async Task<int> ParsePrefix(DiscordMessage msg)
		{
			DatabaseService databaseService = this.services.GetRequiredService<DatabaseService>();

			GuildSettings settings = await databaseService.GetGuildSettingsAsync(msg.Channel.GuildId);
			if (settings == null)
				settings = await databaseService.CreateGuildSettingsAsync(msg.Channel.GuildId);

			string prefix = settings.prefix;

			if (msg.Content.Length <= prefix.Length)
				return -1;

			if (msg.Content.StartsWith(prefix))
				return prefix.Length;
			else
				return -1;
		}
	}
}