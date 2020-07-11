using System.Text;
using Axion.Common.Extensions;
using Victoria;
using Victoria.Interfaces;

namespace Axion.Common.Utilities
{
	public static class CommandUtilities
	{
		public static string GetNearestTracksAsString(DefaultQueue<IQueueable> queue)
		{
			if (queue.Count == 0)
				return "";

			var s = new StringBuilder();

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
