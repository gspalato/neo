using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;

namespace Arpa.Services
{
	public class CommandService
	{
		private readonly DiscordClient client;
		private readonly CommandsNextExtension commands;
		public InteractivityExtension interactivity;

		public CommandService(
			DiscordClient client,
			IServiceProvider services)
		{
			this.client = client;

			this.commands = client.UseCommandsNext(new CommandsNextConfiguration
			{
				EnableDefaultHelp = false,
				EnableDms = false,
				StringPrefixes = new string[] { "pls " },
				Services = services
			});

			this.interactivity = client.UseInteractivity(new InteractivityConfiguration { });
		}

		public void InstallCommandsAsync()
		{
			try
			{
				this.commands.RegisterCommands(Assembly.GetEntryAssembly());
			}
			catch { }
		}
	}
}