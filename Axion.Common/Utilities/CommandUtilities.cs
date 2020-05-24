using Axion.Core.Extensions;
using System;
using System.Text;
using Victoria;
using Victoria.Interfaces;

namespace Axion.Core.Utilities
{
	public static class CommandUtilities
	{
		public static string GetNearestTracksAsString(DefaultQueue<IQueueable> queue)
		{
			if (queue.Count == 0)
				return "";

			StringBuilder s = new StringBuilder();

			int elapsed = 0;
			foreach (LavaTrack track in queue.Items)
			{
				if (elapsed >= 5)
					break;

				s.Append($"{++elapsed}. [{track.Title.TruncateAndSanitize()}]({track.Url})");
				s.Append("\n");
			}

			int remaining = queue.Count - elapsed;
			if (queue.Count > 5)
				s.Append($"and {remaining} more track{(remaining > 1 ? "s" : "")}...");

			return s.ToString();
		}
    }
}
