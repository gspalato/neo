using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Muon.Kernel.Structures;
using Muon.Kernel.Utilities;
using Qmmands;
using System;
using System.Threading.Tasks;

using Utils = Muon.Kernel.Utilities;

namespace Muon.Commands
{
	[Category("Fun")]
	[Description("🎉")]
	public class Fun : MuonModule
	{
		[Command("hug")]
		[Description("Hug someone! **(っ´▽`)っ**")]
		[IgnoresExtraArguments]
		public async Task HugAsync(IGuildUser user)
		{
			Console.WriteLine(user);

			string who;
			if (user.Id == Context.User.Id)
				who = "themself";
			else if (user.Id == Context.Client.CurrentUser.Id)
				who = "me";
			else
				who = user.Mention;

			string image = Utils::CommandUtilities.GetHugGif(Context.ServiceProvider.GetService<Random>());
			EmbedBuilder embed = new EmbedBuilder()
				.WithDescription($"{Context.User.Mention} hugged {who}! **(っ´▽`)っ**")
				.WithInfo()
				.WithImageUrl(image);

			await SendEmbedAsync(embed);
		}
	}
}