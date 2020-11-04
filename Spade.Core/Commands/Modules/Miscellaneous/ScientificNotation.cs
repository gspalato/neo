using Spade.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Spade.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Group("scn")]
	public class ScientificNotation : SpadeModule
	{
		[Command]
		public async Task ScientificNotationAsync(double n)
		{
			var notation = n.ToString($"E{n.ToString(CultureInfo.InvariantCulture).Length - 1}");
			var split = notation.Split("E");

			const string superscriptDigits = "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";

			static string Normalize(string d) =>
				d.TrimStart(new[] { '0' }).TrimEnd('0', ',', '.');

			string ToSuperscript(string d)
			{
				return new string(
					Convert.ToInt32(d)
						.ToString()
						.Select(x => superscriptDigits[x - '0'])
						.ToArray());
			}

			var significant = Normalize(split[0]);
			var exponent = ToSuperscript(split[1]);

			await Context.ReplyAsync($"```{significant} * 10{exponent}```");
		}
	}
}
