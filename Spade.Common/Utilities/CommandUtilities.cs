using System.Text;
using Spade.Common.Extensions;
using Victoria;

namespace Spade.Common.Utilities
{
	public static class CommandUtilities
	{
		public static string GetNearestTracksAsString(DefaultQueue<LavaTrack> queue)
		{
			if (queue.Count == 0)
				return "";

			StringBuilder s = new();

			var elapsed = 0;
			foreach (var queueable in queue.Items)
			{
				var track = (LavaTrack)queueable;
				if (elapsed >= 5)
					break;

				s.Append($"{++elapsed}. [{track.Title.TruncateAndSanitize()}]({track.Url})");
				s.Append("\n");
			}

			var remaining = queue.Count - elapsed;
			if (queue.Count > 5)
				s.Append($"and {remaining} more track{(remaining > 1 ? "s" : "")}...");

			return s.ToString();
		}
    }
}
