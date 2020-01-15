using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

using Arpa;


namespace Arpa.Commands
{
	public partial class Miscellaneous : BaseCommandModule
	{
		[Command("ping")]
		[Description("Gives you the API latency.")]
		public async Task PingAsync(CommandContext ctx)
		{
			Stopwatch sw = new Stopwatch();
			DiscordUser user = ctx.User;

			sw.Start();
			DiscordMessage msg = await ctx.RespondAsync(content: "Measuring...");
			sw.Stop();

			try
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
				embed.WithTitle("🏓 Pong!");
				embed.WithColor(new DiscordColor(0x2A8EF4));
				embed.AddField("API Latency", "```" + ctx.Client.Ping + "ms```", true);
				embed.AddField("Bot Latency", "```" + sw.ElapsedMilliseconds + "ms```", true);

				Console.WriteLine(embed == null);

				await msg.ModifyAsync(content: null, embed: embed.Build());
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}