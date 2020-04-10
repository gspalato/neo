using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Muon.Kernel.Structures;
using Muon.Kernel.Utilities;
using Muon.Services;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

using utils = Muon.Kernel.Utilities;

namespace Muon.Commands.Modules
{
	[Category("Music")]
	[Description("Feel the vibe. B)")]
	public sealed class Music : MuonModule
	{
		private readonly LavaNode _lavaNode;
		private readonly IMusicService _musicService;
		private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

		public Music(IServiceProvider services) // Construtor
		{
			_lavaNode = services.GetRequiredService<LavaNode>();
			_musicService = services.GetRequiredService<IMusicService>();
		}

		[Command("join")]
		[IgnoresExtraArguments]
		public async Task JoinAsync()
		{
			if (_lavaNode.HasPlayer(Context.Guild))
			{
				await Context.ReplyAsync("I'm already connected to a voice channel!");
				return;
			}

			var voiceState = Context.User as IVoiceState;

			if (voiceState?.VoiceChannel == null)
			{
				await Context.ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			try
			{
				await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
				await Context.ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("play")]
		public async Task PlayAsync([Remainder] string query)
		{
			if (Context.User.VoiceChannel is null)
			{
				await SendDefaultEmbedAsync("You must be in a voice channel!");
				return;
			}

			if (string.IsNullOrEmpty(query))
			{
				await SendDefaultEmbedAsync("Please provide search terms.");
				return;
			}

			LavaPlayer player;
			if (_lavaNode.TryGetPlayer(Context.Guild, out player))
			{
				if (player.VoiceChannel.Id != Context.User.VoiceChannel.Id)
				{
					Console.WriteLine("Already connected!");
					return;
				}
			}
			else
				player = await _lavaNode.JoinAsync(Context.User.VoiceChannel, Context.Channel);

			var search = await _lavaNode.SearchYouTubeAsync(query);
			if (search.LoadStatus == LoadStatus.LoadFailed ||
				search.LoadStatus == LoadStatus.NoMatches)
			{
				await Context.ReplyAsync($"Couldn't find video.");
				return;
			}

			var track = search.Tracks.FirstOrDefault();

			if (player.PlayerState == PlayerState.Playing
				|| player.PlayerState == PlayerState.Paused)
			{
				player.Queue.Enqueue(track);

				await SendDefaultEmbedAsync($"Queued **[{track.Title.TruncateAndEscape()}]({track.Url})**");
			}
			else
			{
				await player.PlayAsync(track);

				await SendDefaultEmbedAsync(":notes: Now Playing",
					$"**[{track.Title.TruncateAndEscape()}]({track.Url})**");
			}
		}

		[Command("pause")]
		[IgnoresExtraArguments]
		public async Task PauseAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("I cannot pause when I'm not playing anything!");
				return;
			}

			try
			{
				await player.PauseAsync();

				await Context.ReplyAsync($"Paused song.");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("resume")]
		[IgnoresExtraArguments]
		public async Task ResumeAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Paused)
			{
				await SendDefaultEmbedAsync("The song's not paused!");
				return;
			}

			try
			{
				await player.ResumeAsync();

				await SendDefaultEmbedAsync("Resumed song.");
				return;
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("stop")]
		[IgnoresExtraArguments]
		public async Task StopAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState == PlayerState.Stopped)
			{
				await SendDefaultEmbedAsync("The song's already stopped!");
				return;
			}

			try
			{
				await player.StopAsync();

				await SendDefaultEmbedAsync("Stopped playing.");
				return;
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("leave")]
		[IgnoresExtraArguments]
		public async Task LeaveAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			var voiceChannel = (Context.User as IVoiceState).VoiceChannel ?? player.VoiceChannel;
			if (voiceChannel == null)
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			try
			{
				await _lavaNode.LeaveAsync(voiceChannel);
				return;
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}


		[Command("skip")]
		[IgnoresExtraArguments]
		public async Task SkipAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("Nothing's playing right now.");
				return;
			}

			try
			{
				var currentTrack = await player.SkipAsync();

				await SendDefaultEmbedAsync(":notes: Now Playing",
					$"**[{currentTrack.Title.TruncateAndEscape()}]({currentTrack.Url})**");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("seek")]
		[IgnoresExtraArguments]
		public async Task SeekAsync(TimeSpan timeSpan)
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("I'm not playing anything!.");
				return;
			}

			try
			{
				await player.SeekAsync(timeSpan);

				await SendDefaultEmbedAsync($"Seeked to `{timeSpan.TotalSeconds}s`");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}
		[Command("seek")]
		[IgnoresExtraArguments]
		public async Task SeekAsync(double seconds) =>
			await SeekAsync(TimeSpan.FromSeconds(seconds));

		[Command("volume")]
		[IgnoresExtraArguments]
		public async Task VolumeAsync(ushort volume)
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			try
			{
				await player.UpdateVolumeAsync(volume);
				await SendDefaultEmbedAsync($"Volume set to `{volume}%`");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}
		[Command("volume")]
		[IgnoresExtraArguments]
		public async Task VolumeAsync(double volume) =>
			await VolumeAsync(Convert.ToUInt16(volume));

		[Command("genius", "lyrics")]
		[IgnoresExtraArguments]
		public async Task ShowGeniusLyricsAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("Nothing's playing right now.");
				return;
			}

			var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
			if (string.IsNullOrWhiteSpace(lyrics))
			{
				await SendDefaultEmbedAsync($"No lyrics found for {player.Track.Title}");
				return;
			}

			var splitLyrics = lyrics.Split('\n');
			var stringBuilder = new StringBuilder();
			foreach (var line in splitLyrics)
			{
				if (Range.Contains(stringBuilder.Length))
				{
					await Context.ReplyAsync($"```{stringBuilder}```");
					stringBuilder.Clear();
				}
				else
				{
					stringBuilder.AppendLine(line);
				}
			}

			await Context.ReplyAsync($"```{stringBuilder}```");
		}

		[Command("nowplaying", "np", "now-playing")]
		[IgnoresExtraArguments]
		public async Task NowPlayingAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("Nothing's playing right now.");
				return;
			}

			var track = player.Track;

			string firstTracks = utils::CommandUtilities.GetNearestTracksAsString(player.Queue);
			string slider = track.GenerateSlider();

			string totalTime = track.Duration.ToHumanDuration();
			string elapsedTime = track.Position.ToHumanDuration();
			string remainingTime = (track.Duration - track.Position).ToHumanDuration();

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("🎶 Now Playing")
				.WithDefaultColor()
				.WithDescription(
					$"**[{track.Title.TruncateAndEscape()}]({track.Url})**"
					+ $"\n`{remainingTime}` remaining.\n\n"
					+ firstTracks
				)
				.WithFooter($"{slider}  {elapsedTime} / {totalTime}");

			await SendEmbedAsync(embed: embed);

		}
	}
}