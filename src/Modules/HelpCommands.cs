using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Arpa;
using Arpa.Services;

namespace Arpa.Modules
{
	public class HelpCommands : ModuleBase<SocketCommandContext>
	{
		public ServiceProvider services;

		public HelpCommands(IServiceProvider services)
		{
			this.services = services as ServiceProvider;
		}

		[Command("help")]
		[Summary("i can show you the ~~world~~ commands!")]
		public async Task HelpAsync()
		{
			Func<string> GetGreeting = () =>
			{
				List<string> greetings = new List<string>
				{
					$"Hello! I'm eggcat, how are you?",
					$"Yodacat, I am. You looking for what are?",
					$"hello. i am eggcat. please pet me."
				};

				return greetings[(new Random()).Next(greetings.Count)];
			};

			EmbedBuilder builder = new EmbedBuilder()
				.WithDescription(GetGreeting())
				.WithColor(0x2A8EF4);

			IEnumerable<ModuleInfo> modules = services.GetRequiredService<CommandService>().Modules;
			foreach (ModuleInfo module in modules)
			{
				IReadOnlyList<CommandInfo> commands = module.Commands;
				IEnumerable<string> cmds = commands.Select(x => $"`{x.Name.ToLower()}`");

				builder.AddField(module.Name, String.Join(", ", cmds.ToArray()));
			}

			await services.GetRequiredService<LoggingService>().LogAsync(
				modules.ToString()
			);

			Embed embed = builder.Build();

			await Context.Channel.SendMessageAsync(embed: embed);
		}
	}
}
