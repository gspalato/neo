using Discord;
using Discord.Rest;
using Muon.Kernel.Structures;
using Muon.Kernel.Utilities;
using Qmmands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Muon.Commands
{
	[Category("Miscellaneous")]
	[Description("Special snowflakes that don't fit on other groups.")]
	public sealed class Miscellaneous : MuonModule
	{
		[Command("echo", "say")]
		public async Task EchoAsync([Remainder] string text) =>
			await Context.ReplyAsync(text).ConfigureAwait(false);

		[Command("fw", "fullwidth")]
		public async Task FullwidthAsync([Remainder] string text)
		{
			string output = "";

			for (int i = 0; i < text.Length; i++)
				if (text[i] >= '!' && text[i] <= '~')
					output += (char)((text[i] - '0') - 0x20 + 0xff00);
				else if (text[i] == ' ')
					output += "　";
				else
					output += text[i];

			await Context.ReplyAsync(content: output);
		}

		[Command("ping")]
		[Description("Gives you the API latency.")]
		public async Task PingAsync()
		{
			Stopwatch sw = new Stopwatch();

			sw.Start();
			RestUserMessage msg = await Context.ReplyAsync(content: "Measuring...");
			sw.Stop();

			try
			{
				EmbedBuilder embed = new EmbedBuilder()
					.WithTitle("🏓 Pong!")
					.WithInfo()
					.AddField("API Latency", "```" + Context.Client.Latency + "ms```", true)
					.AddField("Bot Latency", "```" + sw.ElapsedMilliseconds + "ms```", true);

				await msg.ModifyAsync(props =>
				{
					props.Content = "";
					props.Embed = embed.Build();
				}).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		[Command("scn")]
		public async Task ScientificNotationAsync(double n)
		{
			string notation = n.ToString($"E{n.ToString().Length - 1}");
			string[] split = notation.Split("E");

			string superscriptDigits = "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";

			string Normalize(string d) =>
				d.TrimStart(new Char[] { '0' }).TrimEnd(new Char[] { '0', ',', '.' });

			string ToSuperscript(string d)
			{
				int numeral = Convert.ToInt32(d);

				return new string(numeral.ToString()
					.Select(x => superscriptDigits[x - '0']).ToArray());
			}

			string significant = Normalize(split[0]);
			string exponent = ToSuperscript(split[1]);

			string formatted = $"{significant} * 10{exponent}";

			await Context.ReplyAsync($"```{formatted}```");
		}
	}
}