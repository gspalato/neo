using System;
using System.Diagnostics;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Qmmands;

using Arpa.Structures;


namespace Arpa.Commands
{
	[Name("Miscellaneous")]
	[Description("Special snowflakes that don't fit on other groups.")]
	public class MiscCommands : ModuleBase<ArpaCommandContext>
	{
		[Command("ping")]
		[Description("Gives you the API latency.")]
		public async Task PingAsync()
		{
			Stopwatch sw = new Stopwatch();
			DiscordUser user = Context.User;

			//sw.Start();
			DiscordMessage msg = await Context.Message.Channel.SendMessageAsync(content: "Measuring...");
			//sw.Stop();

			Console.WriteLine(msg);

			DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
				.WithTitle("🏓 Pong!")
				//.WithColor(new DiscordColor(0x2A8EF4))
				.AddField("API Latency", $"```{Context.Client.Ping}ms```", true)
				.AddField("Bot Latency", $"```---ms```", true)
				.WithFooter($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl(ImageFormat.Png))
				.WithTimestamp(msg.Timestamp);

			Console.WriteLine(embed == null);

			await msg.ModifyAsync(content: null, embed: embed.Build());
		}

		[Command("echo")]
		public async Task EchoAsync([Remainder] string text) =>
			await Context.ReplyAsync(text: text);
	}
}