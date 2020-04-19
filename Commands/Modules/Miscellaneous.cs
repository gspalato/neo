using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Muon.Kernel.Structures.Attributes;
using Muon.Kernel.Utilities;
using Qmmands;
using Version = Muon.Kernel.Version;

namespace Muon.Commands
{
	[Category("Miscellaneous")]
	[Description("Special snowflakes that don't fit on other groups.")]
	public sealed class Miscellaneous : MuonModule
	{
		[Command("help")]
		[Description("What you're seeing right now.")]
		public async Task HelpAsync()
		{
			var commandService = Context.ServiceProvider.GetRequiredService<ICommandService>();

			var embed = new EmbedBuilder()
				.WithDescription("owo")
				.WithInfo()
				.WithFooter($"by hinoki_. v{Version.FullVersion}");

			foreach (var m in commandService.GetAllModules())
			{
				var category = m.Attributes
					.First(att => att.GetType() == typeof(CategoryAttribute)) as CategoryAttribute;

				var name = category?.Category;

				var names = m.Commands.Select(c => $"`{c.Name}`");

				embed.AddField(name, string.Join(", ", names.Distinct().ToArray()));
			}

			await SendEmbedAsync(embed);
		}
		[Command("help")]
		[Description("What you're seeing right now.")]
		public async Task HelpAsync(string name)
		{
			var commandService = Context.ServiceProvider.GetRequiredService<ICommandService>();
			var cmd = commandService.FindCommands(name).First().Command;

			var aliases =
				from alias in cmd.Aliases
				select $"`{alias}`";

			var embed = new EmbedBuilder()
				.WithInfo()
				.AddField("Name", cmd.Name, true)
				.AddField("Aliases", string.Join(", ", aliases), true)
				.AddField("Description", cmd.Description);

			await SendEmbedAsync(embed);
		}

		[Command("echo", "say")]
		[RequireOwner]
		public async Task EchoAsync([Remainder] string text) =>
			await Context.ReplyAsync(text);

		[Command("quote", "q")]
		[Description("Quote someone's message.")]
		public async Task QuoteAsync(IMessage message)
		{
			var embedAuthor = new EmbedAuthorBuilder()
				.WithName(message.Author.Username)
				.WithIconUrl(message.Author.GetAvatarUrl());

			var embed = new EmbedBuilder()
				.WithAuthor(embedAuthor)
				.WithDefaultColor()
				.WithDescription(message.Content + $"\n\n[Jump To]({message.GetJumpUrl()})")
				.WithTimestamp(message.Timestamp);

			await SendEmbedAsync(embed);
		}

		[Command("ping")]
		[Description("Gives you the API latency.")]
		public async Task PingAsync()
		{
			Stopwatch sw = new Stopwatch();

			sw.Start();
			IUserMessage msg = await Context.ReplyAsync(content: "Measuring...");
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

		[Command("fw", "fullwidth")]
		public async Task FullwidthAsync([Remainder] string text)
		{
			string output = "";

			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				if (0x0020 < c && c < 0x007F)
					output += (char)(0xFF00 + (c - 0x0020));
				else if (c == 0x0020)
					output += (char)0x3000;
				else
					output += text[i];
			}

			await Context.ReplyAsync(content: output);
		}

		[Command("scn")]
		public async Task ScientificNotationAsync(double n)
		{
			string notation = n.ToString($"E{n.ToString().Length - 1}");
			string[] split = notation.Split("E");

			string superscriptDigits = "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";

			string Normalize(string d) =>
				d.TrimStart(new[] { '0' }).TrimEnd('0', ',', '.');

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