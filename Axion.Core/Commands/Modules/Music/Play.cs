using Axion.Common.Extensions;
using Axion.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Rest;

namespace Axion.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Play them tunes. B)")]
	[Group("play")]
	public class Play : AxionModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
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

			if (LavaNode.TryGetPlayer(Context.Guild, out var player))
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
						player = await LavaNode.JoinAsync(Context.User.VoiceChannel, Context.Channel);
					}
				}
			}
			else
				player = await LavaNode.JoinAsync(Context.User.VoiceChannel, Context.Channel);


			var youtubeRegex = new Regex(@"^((http(s)?:\/\/)?)(www\.)?((youtube\.com\/)|(youtu.be\/))[\S]+$");

			SearchResponse search;
			if (youtubeRegex.IsMatch(query))
				search = await LavaNode.SearchAsync(query);
			else
				search = await LavaNode.SearchYouTubeAsync(query);


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

							await SendDefaultEmbedAsync(":notes: now playing",
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

							await SendDefaultEmbedAsync(":notes: now playing",
								$"**[{track.Title.TruncateAndSanitize()}]({track.Url})**");
						}

						break;
					}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
