using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;

using Qmmands;

using Arpa;
using Arpa.Entities;
using Arpa.Structures;


namespace Arpa.Commands
{
	[Name("Fun")]
	[Description("🎉")]
	public class FunCommands : ModuleBase<ArpaCommandContext>
	{
		[Command("hug")]
		[Description("Hug someone! **(っ´▽`)っ**")]
		[IgnoresExtraArguments]
		public async Task HugAsync(DiscordUser user)
		{
			Console.WriteLine(user);

			string who;
			if (user.Id.Equals(Context.User.Id))
				who = "themself";
			else if (user.Id.Equals(Context.Client.CurrentUser.Id))
				who = "me";
			else
				who = user.Mention;

			//string url = this.GetHugImage(ctx);
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
				.WithDescription($"{Context.User.Username} hugged {who}! **(っ´▽`)っ**")
				.WithColor(new DiscordColor(0x2A8EF4));
			//	.WithImageUrl(url);

			//Console.WriteLine($"img: {url}");

			await Context.Channel.SendMessageAsync(embed: embed.Build());
		}
	}
}