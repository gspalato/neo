using Axion.Core.Services;
using Axion.Core.Structures.Attributes;
using Axion.Core.Utilities;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axion.Commands.Modules
{
	[Category("Miscellaneous")]
	[Description("Special snowflakes that don't fit on other groups.")]
	public sealed class Miscellaneous : AxionModule
	{
		private IDocumentationService documentationService;

		public Miscellaneous(IServiceProvider services)
		{
			documentationService = services.GetRequiredService<IDocumentationService>();
		}

		[Command("help")]
		[Description("What you're seeing right now.")]
		public async Task HelpAsync()
		{
			var embed = new EmbedBuilder()
				.WithDescription("")
				.WithInfo()
				.WithFooter($"by hinoki_. v{Axion.Version.FullVersion}");

			foreach (Module m in CommandService.GetAllModules())
			{
				var attribute = m.Attributes
					.First(att => att is CategoryAttribute) as CategoryAttribute;

				var category = attribute?.Category;
				var commands = m.Commands
					.OrderBy(n => n)
					.Select(c => $"`{c.Name}`");

				if (commands.Any(x => x is null))
					return;

				embed.AddField(category, string.Join(", ", commands.Distinct().ToArray()));
			}

			await SendEmbedAsync(embed);
		}
		[Command("help")]
		[Description("What you're seeing right now.")]
		public async Task HelpAsync(string name)
		{
			var cmd = CommandService.FindCommands(name).First().Command;

			var aliases = cmd.Aliases
				.OrderBy(alias => alias)
				.Select(alias => $"`{alias}`");

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
			var sw = new Stopwatch();

			sw.Start();
			var msg = await Context.ReplyAsync(content: "Measuring...");
			sw.Stop();

			var embed = new EmbedBuilder()
				.WithTitle("🏓 Pong!")
				.WithInfo()
				.AddField("API Latency", "```" + Context.Client.Latency.ToString() + "ms```", true)
				.AddField("Bot Latency", "```" + sw.ElapsedMilliseconds.ToString() + "ms```", true);

			await msg.ModifyAsync(props =>
			{
				props.Content = "";
				props.Embed = embed.Build();
			}).ConfigureAwait(false);
		}

		[Command("fw", "fullwidth")]
		public async Task FullwidthAsync([Remainder] string text)
		{
			var output = "";

			foreach (var c in text)
			{
				if (0x0020 < c && c < 0x007F)
					output += (char)(0xFF00 + (c - 0x0020));
				else if (c == 0x0020)
					output += (char)0x3000;
				else
					output += c;
			}

			await Context.ReplyAsync(content: output);
		}

		[Command("scn")]
		public async Task ScientificNotationAsync(double n)
		{
			var notation = n.ToString($"E{n.ToString(CultureInfo.InvariantCulture).Length - 1}");
			var split = notation.Split("E");

			const string superscriptDigits = "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";

			static string Normalize(string d) =>
				d.TrimStart(new[] { '0' }).TrimEnd('0', ',', '.');

			string ToSuperscript(string d)
			{
				var numeral = Convert.ToInt32(d);

				return new string(numeral.ToString()
					.Select(x => superscriptDigits[x - '0']).ToArray());
			}

			var significant = Normalize(split[0]);
			var exponent = ToSuperscript(split[1]);

			var formatted = $"{significant} * 10{exponent}";

			await Context.ReplyAsync($"```{formatted}```");
		}

		[Command("docs")]
		[Description("Search for .NET Core documentation.")]
		public async Task GetDocumentationAsync(
			[Remainder] [Description("The term to search for in the documentation.")] string term)
		{
			Regex reg = new Regex("^[0-9A-Za-z.<>]$");
			foreach (char c in term)
			{
				if (!reg.IsMatch(c.ToString()))
				{
					string s = (c == '\\') ? @"\\" : c.ToString();
					await Context.ReplyAsync($" '{s}' character is not allowed in the search, please try again.");
					return;
				}
			}

			var response = await documentationService.GetDocumentationResultsAsync(term);

			if (response.Count == 0)
			{
				await Context.ReplyAsync("Could not find documentation for your requested term.");
				return;
			}

			var embedCount = 0;

			var stringBuild = new StringBuilder();

			foreach (var res in response.Results.Take(3))
			{
				embedCount++;
				stringBuild.AppendLine($"> **[{res.ItemKind}: {res.DisplayName}]({res.Url})**");
				stringBuild.AppendLine($"{res.Description}");
				stringBuild.AppendLine();

				if (embedCount == 3)
				{
					stringBuild.Append(
						$"{embedCount}/{response.Results.Count} results shown ~ [Click here for more results](https://docs.microsoft.com/dotnet/api/?term={term})"
					);
				}
			}
			var buildEmbed = new EmbedBuilder()
				.WithDescription(stringBuild.ToString())
				.WithInfo();

			var message = await SendEmbedAsync(buildEmbed);
		}
	}
}