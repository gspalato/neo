using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;

using Muon.Core.Structures;
using Muon.Core.Utilities;
using Muon.Services;


namespace Muon.Commands
{
	public partial class Music : BaseCommandModule
	{
		[Command("nowplaying")]
		[Aliases("np", "now-playing")]
		public async Task NowPlayingAsync(CommandContext ctx)
		{
			MusicService musicService = ctx.Services.GetRequiredService<MusicService>();
			Player player = musicService.GetPlayer(ctx.Guild) as Player;

			if (player.connection == null || !player.isPlaying)
			{
				await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
					.WithDescription("Nothing's playing right now.")
					.WithDefaultColor()
					.WithTimestamp(ctx.Message.Timestamp)
					.Build());

				return;
			}

			LavalinkTrack current = player.current;
			LavalinkPlayerState state = player.connection.CurrentState;

			string firstTracks = this.GetNearestTracksAsString(player.queue);
			string slider = this.GenerateSlider(current, (int)state.PlaybackPosition.TotalSeconds);

			string totalTime = this.ToHumanReadableTimeSpan(
				Convert.ToInt64(current.Length.TotalMilliseconds));
			string elapsedTime = this.ToHumanReadableTimeSpan(
				Convert.ToInt64(state.PlaybackPosition.TotalMilliseconds));
			string remainingTime = this.ToHumanReadableTimeSpan(
				Convert.ToInt64(
					current.Length.TotalMilliseconds - state.PlaybackPosition.TotalMilliseconds));

			DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
				.WithTitle("🎶 Now Playing")
				.WithDefaultColor()
				.WithDescription(
					$"**[{current.Title.TruncateAndEscape()}]({current.Uri})**"
					+ $"\n`{remainingTime}` remaining.\n\n"
					+ firstTracks
				)
				.WithFooter($"{slider}  {elapsedTime} / {totalTime}");

			await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);

		}

		private string GetNearestTracksAsString(ConcurrentQueue<LavalinkTrack> queue)
		{
			if (queue.Count() == 0)
				return "";

			StringBuilder s = new StringBuilder();

			int elapsed = 0;
			foreach (LavalinkTrack track in queue)
			{
				if (elapsed >= 5)
					break;

				s.Append($"{++elapsed}. [{track.Title.TruncateAndEscape()}]({track.Uri})");
				s.Append("\n");
			}

			int remaining = queue.Count - elapsed;
			if (queue.Count() > 5)
				s.Append($"and {remaining} more track{(remaining > 1 ? "s" : "")}...");

			return s.ToString();
		}

		private string GenerateSlider(LavalinkTrack track, int position)
		{
			StringBuilder slider = new StringBuilder();
			for (int i = 0; i <= 19; i++)
				slider.Append("▬");

			double sliderPosition = position * 20 / track.Length.TotalSeconds;
			int roundSliderPosition = (int)Math.Floor(sliderPosition);
			slider.Insert((roundSliderPosition <= 0) ? 0 : roundSliderPosition - 1, "🔵");

			return slider.ToString();
		}
	}
}