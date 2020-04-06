using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Muon.Commands
{
    public partial class Miscellaneous : BaseCommandModule
    {
        [Command("fw")]
        [Aliases("fullwidth")]
        public async Task FullwidthAsync(CommandContext ctx, [RemainingText] string text)
        {
            string output = "";

            for (int i = 0; i < text.Length; i++)
                if (text[i] >= '!' && text[i] <= '~')
                    output += (char)((text[i] - '0') - 0x20 + 0xff00);
                else if (text[i] == ' ')
                    output += "　";
                else
                    output += text[i];

            await ctx.RespondAsync(content: output);
        }
    }
}
