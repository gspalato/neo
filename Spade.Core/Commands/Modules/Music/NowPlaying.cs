using Qmmands;
using Spade.Common.Extensions;
using Spade.Core.Structures.Attributes;
using System;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Interfaces;

namespace Spade.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Check out what's playing right now.")]
	[Group("nowplaying", "np", "now-playing")]
	public sealed class NowPlaying : SpadeModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task ExecuteAsync()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState is not PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("Nothing's playing right now.");
				return;
			}

			var track = player.Track;

			var firstTracks = GetNearestTracksAsString(player.Queue);
			var slider = GenerateSlider(track);

			var totalTime = track.Duration.ToHumanDuration();
			var elapsedTime = track.Position.ToHumanDuration();
			var remainingTime = (track.Duration - track.Position).ToHumanDuration();

			var description = new StringBuilder();

			description.AppendLine($"**[{track.Title.TruncateAndSanitize()}]({track.Url})**");
			description.AppendLine($"`{remainingTime}` remaining.\n");
			description.Append(firstTracks);

			var embed = CreateDefaultEmbed("🎶 Now Playing", description.ToString())
				.WithFooter($"{slider}  {elapsedTime} / {totalTime}");

			await SendEmbedAsync(embed);
		}

		private string GetNearestTracksAsString(DefaultQueue<IQueueable> queue)
		{
			if (queue.Count == 0)
				return "";

			var s = new StringBuilder();

			var elapsed = 0;
			foreach (var queueable in queue)
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

		private string GenerateSlider(LavaTrack track)
		{
			StringBuilder slider = new StringBuilder();
			for (int i = 0; i <= 29; i++)
				slider.Append("▬");

			double sliderPosition = track.Position.TotalSeconds * 30 / track.Duration.TotalSeconds;
			int roundSliderPosition = (int)Math.Round(sliderPosition);
			slider.Insert((roundSliderPosition <= 0) ? 0 : roundSliderPosition - 1, "\ud83d\udd35");

			return slider.ToString();
		}
	}
}