using Axion.Structures.Attributes;
using Axion.Core.Utilities;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;
using Utils = Axion.Core.Utilities;

namespace Axion.Commands.Modules
{
	[Category("Fun")]
	[Description("🎉")]
	public class Fun : AxionModule
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

			var image = Utils::CommandUtilities.GetHugGif(Context.ServiceProvider.GetService<Random>());
			var embed = new EmbedBuilder()
				.WithDescription($"{Context.User.Mention} hugged {who}! **(っ´▽`)っ**")
				.WithInfo()
				.WithImageUrl(image);

			await SendEmbedAsync(embed);
		}
	}
}