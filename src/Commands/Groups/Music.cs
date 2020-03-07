using System;
using System.Text;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Muon;
using Muon.Core.Structures;

namespace Muon.Commands
{
	[Category("Music")]
	[Description("Drop the beat.")]
	public partial class Music : BaseCommandModule
	{
		private string ToHumanReadableTimeSpan(long milliseconds)
		{
			if (milliseconds == 0)
				return "0s";

			StringBuilder total = new StringBuilder();
			Action<int, string, int> Add = (val, displayunit, zeroplaceholder) =>
			{
				if (val <= 0)
					return;

				total.Append(string.Format("{0:D" + zeroplaceholder.ToString() + "}" + displayunit, val));
				total.Append(" ");
			};

			TimeSpan t = TimeSpan.FromMilliseconds(milliseconds);

			Add(t.Days, "d", 1);
			Add(t.Hours, "h", 1);
			Add(t.Minutes, "m", 1);
			Add(t.Seconds, "s", 1);

			return total.ToString().Trim();
		}
	}
}