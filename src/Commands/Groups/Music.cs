using System;
using System.Text;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Arpa;
using Arpa.Structures;

namespace Arpa.Commands
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
			Action<int, string, int> add = (val, displayunit, zeroplaceholder) =>
			{
				if (val <= 0)
					return;

				total.Append(string.Format("{0:D" + zeroplaceholder.ToString() + "}" + displayunit, val));
				total.Append(" ");
			};

			TimeSpan t = TimeSpan.FromMilliseconds(milliseconds);

			add(t.Days, "d", 1);
			add(t.Hours, "h", 1);
			add(t.Minutes, "m", 1);
			add(t.Seconds, "s", 1);

			return total.ToString().Trim();
		}
	}
}