using Axion.Core.Structures.Attributes;
using Axion.Core.Structures.Interactivity;
using Axion.Core.Utilities;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Interfaces;
using Victoria.Responses.Rest;
using Utils = Axion.Core.Utilities;

namespace Axion.Commands.Modules
{
	[Category("Music")]
	[Description("Feel the vibe. B)")]
	public sealed class Music : AxionModule
	{
		private readonly LavaNode _lavaNode;

		public Music(IServiceProvider services)
		{
			_lavaNode = services.GetRequiredService<LavaNode>();
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

			if (voiceState?.VoiceChannel is null)
			{
				await Context.ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			try
			{
				await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel);
				await Context.ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("play")]
		[Description("Play them tunes. B)")]
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

			if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				if (player.VoiceChannel.Id != Context.User.VoiceChannel.Id)
				{
					var users = await player.VoiceChannel.GetUsersAsync().FirstAsync();

					if (users.Count > 1)
					{
						await SendDefaultEmbedAsync("I'm already connected at another channel!");
						return;
					}
					else if (users.Count == 1)
					{
						await player.StopAsync();
						player = await _lavaNode.JoinAsync(Context.User.VoiceChannel, Context.Channel);
					}
				}
			}
			else
				player = await _lavaNode.JoinAsync(Context.User.VoiceChannel, Context.Channel);


			var youtubeRegex = new Regex(@"^((http(s)?:\/\/)?)(www\.)?((youtube\.com\/)|(youtu.be\/))[\S]+$");

			SearchResponse search;
			if (youtubeRegex.IsMatch(query))
				search = await _lavaNode.SearchAsync(query);
			else
				search = await _lavaNode.SearchYouTubeAsync(query);


			switch (search.LoadStatus)
			{
				case LoadStatus.LoadFailed:
				case LoadStatus.NoMatches:
					await SendDefaultEmbedAsync("Couldn't find video.");
					return;

				case LoadStatus.PlaylistLoaded:
					{
						var tracks = search.Tracks.ToList();

						var mainTrackIndex = search.Playlist.SelectedTrack;
						var mainTrack = tracks.ElementAt(mainTrackIndex >= 0 ? mainTrackIndex : 0);

						tracks.Remove(mainTrack);
						tracks.Insert(0, mainTrack);

						if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
						{
							foreach (var track in tracks)
								player.Queue.Enqueue(track);

							await SendDefaultEmbedAsync($"Queued **{search.Playlist.Name.TruncateAndSanitize()}**");
						}
						else
						{
							await player.PlayAsync(mainTrack);
							tracks.RemoveAt(0);

							foreach (var track in tracks)
								player.Queue.Enqueue(track);

							await SendDefaultEmbedAsync(":notes: Now Playing",
								$"**[{mainTrack.Title.TruncateAndSanitize()}]({mainTrack.Url})**\n"
								+ $"from playlist **{search.Playlist.Name.TruncateAndSanitize()}**");
						}

						break;
					}

				case LoadStatus.SearchResult:
				case LoadStatus.TrackLoaded:
					{
						var track = search.Tracks.FirstOrDefault();
						if (track is null)
							return;

						if (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
						{
							player.Queue.Enqueue(track);

							await SendDefaultEmbedAsync($"Queued **[{track.Title.TruncateAndSanitize()}]({track.Url})**");
						}
						else
						{
							await player.PlayAsync(track);

							await SendDefaultEmbedAsync(":notes: Now Playing",
								$"**[{track.Title.TruncateAndSanitize()}]({track.Url})**");
						}

						break;
					}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		[Command("pause")]
		[Description("Pause them tunes. B(")]
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

				await Context.ReplyAsync("Paused song.");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("resume")]
		[Description("Unpause them tunes. B)")]
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
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("stop")]
		[Description("Stop them tunes. B(")]
		[IgnoresExtraArguments]
		public async Task StopAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState is PlayerState.Stopped)
			{
				await SendDefaultEmbedAsync("The song's already stopped!");
				return;
			}

			try
			{
				await player.StopAsync();
				await SendDefaultEmbedAsync("Stopped playing.");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message, null);
			}
		}

		[Command("leave")]
		[Description("Leave the voice channel. B(")]
		[IgnoresExtraArguments]
		public async Task LeaveAsync()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			var voiceChannel = Context.User.VoiceChannel ?? player.VoiceChannel;
			if (voiceChannel is null)
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			try
			{
				await _lavaNode.LeaveAsync(voiceChannel);
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message, null);
			}
		}


		[Command("skip")]
		[Description("Skippity Skoppity, your tunes are now my property.")]
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
					$"**[{currentTrack.Title.TruncateAndSanitize()}]({currentTrack.Url})**");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command("seek")]
		[Description("Skip to a certain time in the song.")]
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
		[Description("Skip to a certain time in the song.")]
		[IgnoresExtraArguments]
		public async Task SeekAsync(double seconds) =>
			await SeekAsync(TimeSpan.FromSeconds(seconds));

		[Command("volume", "vol", "v")]
		[Description("Set the beat's volume!")]
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
		[Command("volume", "vol", "v")]
		[Description("Set the beat's volume!")]
		[IgnoresExtraArguments]
		public async Task VolumeAsync(double volume) =>
			await VolumeAsync(Convert.ToUInt16(volume));

		[Command("genius", "lyrics")]
		[Description("But I can't help, giving you them lyrics..")]
		[IgnoresExtraArguments]
		public async Task ShowGeniusLyricsAsync()
		{
			var range = Enumerable.Range(1900, 2000).ToArray();

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
				if (range.Contains(stringBuilder.Length))
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

		[Command("queue")]
		[Description("Check out what tunes are going to play next!")]
		[IgnoresExtraArguments]
		public async Task QueueAsync()
		{
			try
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

				IList<Embed> pages = new List<Embed>();

				var queue = player.Queue.Items.Chunk(7);

				var totalTrackNumber = 0;
				var chunks = queue as IQueueable[][] ?? queue.ToArray();

				if (chunks.Count() == 0)
				{
					await SendDefaultEmbedAsync("There are no tracks next.");
					return;
				}

				for (var chunkNumber = 0; chunkNumber < chunks.Count(); chunkNumber++)
				{
					var chunk = chunks.ElementAt(chunkNumber);
					var description = new StringBuilder();

					if (chunk is null)
						break;

					for (var trackNumber = 0; trackNumber < chunk.Count(); trackNumber++)
					{
						if (!(chunk.ElementAt(trackNumber) is LavaTrack track))
							return;

						description.Append($"{++totalTrackNumber}. [**{track.Title.TruncateAndSanitize()}**]({track.Url})\n");
					}

					var embed = CreateDefaultEmbed($":musical_score:  Queue | Page {chunkNumber + 1}/{chunks.Count()}",
						description.ToString());

					pages.Add(embed.Build());
				}

				var pagedMessage = new PaginatedMessage(Context.Client, Context.Message.Author, pages.ToArray());
				await pagedMessage.Send(Context.Channel);
			}
			catch (Exception e)
			{
				Console.WriteLine($"{e.Message}\n{e.StackTrace}");
			}
		}

		[Command("shuffle")]
		[Description("Shuffle those tunes! B)")]
		[IgnoresExtraArguments]
		public async Task ShuffleAsync()
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

			player.Queue.Shuffle();

			await SendDefaultEmbedAsync("Shuffled queue.");
		}

		[Command("nowplaying", "np", "now-playing")]
		[Description("Check out what's playing right now.")]
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

			var firstTracks = Utils::CommandUtilities.GetNearestTracksAsString(player.Queue);
			var slider = track.GenerateSlider();

			var totalTime = track.Duration.ToHumanDuration();
			var elapsedTime = track.Position.ToHumanDuration();
			var remainingTime = (track.Duration - track.Position).ToHumanDuration();

			var description = $"**[{track.Title.TruncateAndSanitize()}]({track.Url})**"
					+ $"\n`{remainingTime}` remaining.\n\n"
					+ firstTracks;

			var embed = CreateDefaultEmbed("🎶 Now Playing", description)
				.WithFooter($"{slider}  {elapsedTime} / {totalTime}");

			await SendEmbedAsync(embed: embed);

		}
	}
}