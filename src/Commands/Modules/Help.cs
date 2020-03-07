using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Muon.Core;
using Muon.Core.Utilities;

namespace Muon.Commands
{
    public partial class Moderation
    {
        [Command("help")]
        public async Task HelpAsync(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Help")
                .WithInfo()
                .WithFooter($"by ohinoki. v{Version.FullVersion}");

            StringBuilder description = new StringBuilder();

            var sorted = from s in ctx.CommandsNext.RegisteredCommands.Keys
                         orderby s
                         select $"`{s}`";

            embed.WithDescription(description.AppendJoin(", ", sorted).ToString());

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("help")]
        public async Task HelpAsync(CommandContext ctx, string command)
        {
            
        }
    }
}
