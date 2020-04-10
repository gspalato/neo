using System;
using System.Text;
using Victoria;
using Victoria.Interfaces;

namespace Muon.Kernel.Utilities
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

				s.Append($"{++elapsed}. [{track.Title.TruncateAndEscape()}]({track.Url})");
				s.Append("\n");
			}

			int remaining = queue.Count - elapsed;
			if (queue.Count > 5)
				s.Append($"and {remaining} more track{(remaining > 1 ? "s" : "")}...");

			return s.ToString();
		}

		public static string GenerateSlider(LavaTrack track, int position)
		{
			StringBuilder slider = new StringBuilder();
			for (int i = 0; i <= 19; i++)
				slider.Append("▬");

			double sliderPosition = position * 20 / track.Duration.TotalSeconds;
			int roundSliderPosition = (int)Math.Floor(sliderPosition);
			slider.Insert((roundSliderPosition <= 0) ? 0 : roundSliderPosition - 1, "🔵");

			return slider.ToString();
		}
	}
}
