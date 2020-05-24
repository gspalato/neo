using System;
using System.Text;
using Victoria;

namespace Axion.Core.Extensions
{
	public static class LavaTrackExtension
	{
		public static string GenerateSlider(this LavaTrack track)
		{
			StringBuilder slider = new StringBuilder();
			for (int i = 0; i <= 29; i++)
				slider.Append("▬");

			double sliderPosition = track.Position.TotalSeconds * 30 / track.Duration.TotalSeconds;
			int roundSliderPosition = (int)Math.Floor(sliderPosition);
			slider.Insert((roundSliderPosition <= 0) ? 0 : roundSliderPosition - 1, "🔵");

			return slider.ToString();
		}
	}
}
