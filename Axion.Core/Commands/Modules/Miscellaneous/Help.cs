using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Miscellaneous
{
    [Category(Category.Miscellaneous)]
    [Description("What you're seeing right now.")]
    [Group("help")]
    public class Help : AxionModule
    {
        [Command]
        public async Task ExecuteAsync()
        {
            var embed = new EmbedBuilder()
                .WithDescription("")
                .WithInfo()
                .WithFooter($"by hinoki_. v{Version.FullVersion}");

            var modules = CommandService.GetAllModules();
            var categories = new Dictionary<string, List<string>>();

            foreach (var m in modules)
            {
                var attribute = (CategoryAttribute)m.Attributes.First(x => x is CategoryAttribute);
                var group = (GroupAttribute) m.Attributes.First(x => x is GroupAttribute);

                var category = Enum.GetName(typeof(Category), attribute.Category);
                if (category is null)
                    return;

                if (!categories.TryGetValue(category, out var cmds))
                {
                    categories.TryAdd(category, new List<string>());
                    _ = categories.TryGetValue(category, out cmds);
                }

                cmds.Add($"`{group.Aliases[0]}`");
            }

            foreach (var (key, value) in categories.OrderBy(x => x.Key))
            {
                embed.AddField(key, string.Join(", ", value.OrderBy(x => x)));
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

        [Command]
        public async Task ExecuteAsync(Command cmd)
        {
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
    }
}
