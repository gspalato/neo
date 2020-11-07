using Spade.Common.Extensions;
using Spade.Core.Structures.Attributes;
using Spade.Database.Repositories;
using Discord;
using InteractivityNET;
using MongoDB.Bson;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Rest;

namespace Spade.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Check out what tunes are going to play next!")]
	[Group("queue")]
	public class Queue : SpadeModule
	{
		public LavaNode LavaNode { get; set; }
		public IQueueRepository QueueRepository { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task ExecuteAsync()
		{
			try
			{
				if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
				{
					await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
					return;
				}

				if (player.PlayerState != PlayerState.Playing)
				{
					await SendDefaultEmbedAsync("Nothing's playing right now.");
					return;
				}

				var pagedBuilder = new PaginatedMessageBuilder()
					.WithDefaultButtons()
					.WithResponsible(Context.User)
					.WithTemplate(() =>
						new EmbedBuilder()
							.WithDefaultColor());

				var queue = player.Queue.Chunk(7);
				var chunks = queue ?? queue.ToArray();

				if (!chunks.Any())
				{
					await SendDefaultEmbedAsync("There are no tracks next.");
					return;
				}

				var totalTrackNumber = 0;
				for (var chunkNumber = 0; chunkNumber < chunks.Count(); chunkNumber++)
				{
					var chunk = chunks.ElementAt(chunkNumber);
					var description = new StringBuilder();

					if (chunk is null)
						break;

					for (var trackNumber = 0; trackNumber < chunk.Count(); trackNumber++)
					{
						if (chunk.ElementAt(trackNumber) is not LavaTrack track)
							return;

						description.Append($"{++totalTrackNumber}. [**{track.Title.TruncateAndSanitize()}**]({track.Url})\n");
					}

					var page = chunkNumber;
					pagedBuilder.AddPage(template =>
					{
						template
							.WithTitle($":musical_score: queue · page {page} of {chunks.Count()}")
							.WithDescription(description.ToString());
					});
				}

				var pagedMessage = pagedBuilder.Build(Context.Client);
				await pagedMessage.Send(Context.Channel);
			}
			catch (Exception e)
			{
				Console.WriteLine($"{e.Message}\n{e.StackTrace}");
			}
		}

		private void ReadTrackStatusAsync(SearchResponse search, LavaPlayer player, ref int count)
		{
			switch (search.LoadStatus)
			{
				case LoadStatus.LoadFailed:
				case LoadStatus.NoMatches:
					throw new Exception("One of the queue's videos couldn't be found. Please try again.");

				// Won't happen.
				case LoadStatus.PlaylistLoaded:
					break;

				case LoadStatus.SearchResult:
				case LoadStatus.TrackLoaded:
					{
						var track = search.Tracks.FirstOrDefault();
						if (track is null)
							return;

						if (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
						{
							player.Queue.Enqueue(track);
						}
						else
						{
							_ = player.PlayAsync(track);
						}

						count++;

						break;
					}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		[Command("clear")]
		public async Task ClearQueueAsync()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.Queue.Count is 0 && player.Track is not null)
			{
				await SendDefaultEmbedAsync("The queue is empty.");
				return;
			}

			player?.Queue.Clear();

			await Context.Message.AddReactionAsync(new Emoji("✅"));
		}
	}
}
