using Axion.Common.Extensions;
using Axion.Core.Structures.Attributes;
using Axion.Database.Repositories;
using Discord;
using InteractivityNET;
using MongoDB.Bson;
using Qmmands;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Interfaces;
using Victoria.Responses.Rest;

namespace Axion.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Check out what tunes are going to play next!")]
	[Group("queue")]
	public class Queue : AxionModule
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

				var queue = player.Queue.Items.Chunk(7);
				var chunks = queue as IQueueable[][] ?? queue.ToArray();

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
						if (!(chunk.ElementAt(trackNumber) is LavaTrack track))
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

		[Command("code")]
		public async Task LoadByCodeAsync(string code)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			var queue = await QueueRepository.FindAsync(q => q.Id == ObjectId.Parse(code));
			if (queue is null)
			{
				await SendDefaultEmbedAsync("Couldn't find shared queue.");
				return;
			}

			await SendDefaultEmbedAsync("Loading queue...");

			var loaded = 0;
			foreach (var url in queue.Urls)
			{
				var search = await LavaNode.SearchAsync(url);

				try
				{
					ReadTrackStatusAsync(search, player, ref loaded);
				}
				catch (Exception e)
				{
					await SendDefaultEmbedAsync(e.Message);
				}
			}

			await SendDefaultEmbedAsync(":notes: now playing", $"Loaded {loaded} tracks from queue `{queue.Name}`.");
		}

		[Command("create", "save")]
		public async Task CreateAsync([Remainder] string name)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.Queue.Count is 0 && player.Track != null)
			{
				await SendDefaultEmbedAsync("The queue is empty.");
				return;
			}
			if (player.Queue.Count > 20)
			{
				await SendDefaultEmbedAsync("The queue must have less than 20 musics.");
				return;
			}

			if (name.Length > 20)
			{
				await SendDefaultEmbedAsync("The queue name must have less than 20 characters.");
				return;
			}

			name = Regex.Match(name, @"^[a-zA-Z0-9_ ]*$").Value;
			if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
			{
				await SendDefaultEmbedAsync("Invalid name.");
				return;
			}

			var queue = (new[] { player.Track }).Concat(player.Queue.Items);

			if (!(QueueRepository.GetQueueAsync(Context.Guild, name) is null))
			{
				var queueModel = new Database.Entities.Queue
				{
					Name = name,
					GuildId = Context.Guild.Id.ToString(),
					Urls = queue.Select(t => ((LavaTrack)t).Url)
				};

				await QueueRepository.EditAsync(q => q.Name == name, queueModel);
				return;
			}
			else
			{
				await QueueRepository.SaveQueueAsync(Context.Guild, name, queue);
			}

			await SendDefaultEmbedAsync($"Saved queue `{name}`.");
		}

		[Command("load")]
		public async Task LoadAsync([Remainder] string name)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (name.Length > 20)
			{
				await SendDefaultEmbedAsync("Invalid name.");
				return;
			}

			var queue = await QueueRepository.GetQueueAsync(Context.Guild, name);
			if (queue is null)
			{
				await SendDefaultEmbedAsync("Couldn't find saved queue.");
				return;
			}

			await SendDefaultEmbedAsync("Loading queue...");

			var loaded = 0;
			foreach (var url in queue.Urls)
			{
				var search = await LavaNode.SearchAsync(url);

				try
				{
					ReadTrackStatusAsync(search, player, ref loaded);
				}
				catch (Exception e)
				{
					await SendDefaultEmbedAsync(e.Message);
				}
			}

			await SendDefaultEmbedAsync(":notes: now playing", $"Loaded {loaded} tracks from queue `{name}`.");
		}

		[Command("share")]
		public async Task ShareAsync([Remainder] string name)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			var queue = await QueueRepository.GetQueueAsync(Context.Guild, name);
			if (queue is null)
			{
				await SendDefaultEmbedAsync("Couldn't find saved queue.");
				return;
			}

			var code = queue.Id.ToString();
			await SendDefaultEmbedAsync(":musical_score: QShare", $"The queue's share string is\n`{code}`");
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
	}
}
