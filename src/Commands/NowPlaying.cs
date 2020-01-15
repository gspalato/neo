using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;

using Arpa.Services;
using Arpa.Structures;
using Arpa.Utilities;


namespace Arpa.Commands
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
					.WithColor(new DiscordColor(0x2F3136))
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
				.WithColor(new DiscordColor(0xAA0099))
				.WithDescription(
					$"**[{current.Title.TruncateAndEscape(30)}]({current.Uri})**"
					+ $"\n{remainingTime} remaining.\n\n"
					+ firstTracks
				)
				.WithFooter($"{slider}  {elapsedTime} / {totalTime}");

			await ctx.RespondAsync(embed: embed.Build());

		}

		private string GetNearestTracksAsString(ConcurrentQueue<LavalinkTrack> queue)
		{
			if (queue.Count() == 0)
				return "";

			List<string> s = new List<string>();

			int elapsed = 0;
			foreach (LavalinkTrack track in queue)
			{
				if (elapsed >= 5)
					break;

				s.Add($"{++elapsed}. [{track.Title.TruncateAndEscape(30)}]({track.Uri})");
			}

			int remaining = queue.Count - elapsed;
			if (queue.Count() > 5)
				s.Add($"and {remaining} more track{(remaining > 1 ? "s" : "")}...");

			return string.Join("\n", s.ToArray()) ?? "";
		}

		private string GenerateSlider(LavalinkTrack track, int position)
		{
			List<string> slider = new List<string>();
			for (int i = 0; i <= 19; i++)
				slider.Add("▬");

			double sliderPosition = position * 20 / track.Length.TotalSeconds;
			int roundSliderPosition = (int)Math.Floor(sliderPosition);
			slider.Insert((roundSliderPosition <= 0) ? 0 : roundSliderPosition - 1, "🔵");

			return string.Join("", slider.ToArray());
		}

		private string ToHumanReadableTimeSpan(long milliseconds)
		{
			if (milliseconds == 0) return "0ms";

			List<string> tsparts = new List<string>();
			Action<int, string, int> add = (val, displayunit, zeroplaceholder) =>
			{
				if (val <= 0)
					return;

				tsparts.Add(
					string.Format(
						"{0:DL}X".Replace("X", displayunit).Replace("L", zeroplaceholder.ToString()), val
					)
				);
			};

			TimeSpan t = TimeSpan.FromMilliseconds(milliseconds);

			add(t.Days, "d", 1);
			add(t.Hours, "h", 1);
			add(t.Minutes, "m", 1);
			add(t.Seconds, "s", 1);

			return string.Join(" ", tsparts);
		}
	}
}