using Spade.Common.Extensions;
using Spade.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using attributes = Spade.Core.Structures.Attributes;

namespace Spade.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Description("What you're seeing right now.")]
	[Group("help")]
	public class Help : SpadeModule
	{
		[Command]
		public async Task ExecuteAsync()
		{
			var embed = new EmbedBuilder()
				.WithDescription("")
				.WithInfo()
				.WithFooter($"by ace.　|　v{Version.FullVersion}");

			var modules = CommandService.GetAllModules();
			var categories = new Dictionary<string, List<string>>();

			foreach (var m in modules)
			{
				CategoryAttribute categoryAttribute = default;
				try
				{
					categoryAttribute = m.Attributes.First(x => x is CategoryAttribute) as CategoryAttribute;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					continue;
				}

				var category = categoryAttribute.Category.ToString();
				if (category is null)
					continue;

				if (!categories.TryGetValue(category, out var cmds))
				{
					categories.TryAdd(category, new List<string>());
					_ = categories.TryGetValue(category, out cmds);
				}

				cmds?.Add($"`{m.Name.ToLower()}`");
			}

			foreach (var (key, value) in categories.OrderBy(x => x.Key))
			{
				var joined = string.Join(", ", value.OrderBy(x => x));
				embed.AddField(key, joined);
			}

			await SendEmbedAsync(embed);
		}

		[Command]
		public async Task ExecuteAsync(Module m)
		{
			var aliases = m.Aliases
				.OrderBy(alias => alias)
				.Select(alias => $"`{alias}`");

			var embed = new EmbedBuilder()
				.WithInfo()
				.AddField("Name", m.Name, true)
				.AddField("Aliases", string.Join(", ", aliases), true)
				.AddField("Description", m.Description);

			if (m.Commands.Any())
			{
				embed.AddField("Subcommands", string.Join(", ", m.Commands.Select(x => $"`{x.Name}`")));
			}

			await SendEmbedAsync(embed);
		}
	}
}
