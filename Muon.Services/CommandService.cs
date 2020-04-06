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
using Muon.Kernel.Structures;
using Muon.Services;

namespace Muon.Services
{
	public interface ICommandService
	{
		public void InstallCommandsAsync();
	}

	public class CommandService : ICommandService
	{
		public readonly InteractivityExtension _interactivity;

		private readonly DiscordClient _client;
		private readonly CommandsNextExtension _commands;

		private readonly IDatabaseService _databaseService;

		public CommandService(DiscordClient client,
			IDatabaseService databaseService, IServiceProvider services)
		{
			_client = client;
			_databaseService = databaseService;

			_commands = client.UseCommandsNext(new CommandsNextConfiguration
			{
				PrefixResolver = this.ParsePrefix,
				EnableDefaultHelp = false,
				EnableDms = false,
				Services = services,
			});

			_interactivity = client.UseInteractivity(new InteractivityConfiguration { });
		}

		public void InstallCommandsAsync()
		{
			try
			{
				_commands.RegisterCommands(Assembly.GetEntryAssembly());
				_commands.CommandErrored += this.HandleCommandError;
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

			GuildSettings settings = await _databaseService.GetGuildSettingsAsync(msg.Channel.GuildId);
			if (settings == null)
				settings = await _databaseService.CreateGuildSettingsAsync(msg.Channel.GuildId);

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