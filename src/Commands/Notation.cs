using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Muon.Commands
{
    public partial class Miscellaneous : BaseCommandModule
    {
        [Command("scn")]
        [Aliases("scientific-notation", "notation")]
        public async Task ScientificNotationAsync(CommandContext ctx, double n)
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

            await ctx.RespondAsync($"```{formatted}```");
        }
    }
}
