using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Arpa;


namespace Arpa.Commands
{
	[Description("🎉")]
	public class Fun : BaseCommandModule
	{
		[Command("hug")]
		[Description("Hug someone! **(っ´▽`)っ**")]
		public async Task HugAsync(CommandContext ctx, DiscordMember user)
		{
			string who;
			if (user.Id.Equals(ctx.User.Id))
				who = "themself";
			else if (user.Id.Equals(ctx.Client.CurrentUser.Id))
				who = "me";
			else
				who = user.Mention;

			DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
			embed.WithDescription($"{ctx.User.Username} hugged {who}! **(っ´▽`)っ**");
			embed.WithColor(new DiscordColor(0x2A8EF4));

			await ctx.RespondAsync(embed: embed.Build());
		}
	}
}