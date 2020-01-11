using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace Arpa.Services
{
	public class CommandService
	{
		private readonly DiscordClient client;
		private readonly CommandsNextExtension commands;

		public CommandService(
			DiscordClient client,
			ServiceProvider services)
		{
			this.client = client;

			this.commands = client.UseCommandsNext(new CommandsNextConfiguration
			{
				EnableDefaultHelp = false,
				EnableDms = false,
				StringPrefixes = new string[] { "pls " },
				Services = services
			});
		}

		public void InstallCommandsAsync()
		{
			this.commands.RegisterCommands(Assembly.GetEntryAssembly());
		}
	}
}