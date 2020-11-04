using System;
using System.Text;
using Victoria;

namespace Spade.Common.Extensions
{
	public static class LavaTrackExtension
	{
		public static string GenerateSlider(this LavaTrack track)
		{
			StringBuilder slider = new();
			for (int i = 0; i <= 29; i++)
				slider.Append("▬");

			double sliderPosition = track.Position.TotalSeconds * 30 / track.Duration.TotalSeconds;
			int roundSliderPosition = (int)Math.Round(sliderPosition);
			slider.Insert((roundSliderPosition <= 0) ? 0 : roundSliderPosition - 1, "\ud83d\udd35");

			return slider.ToString();
		}
	}
}
