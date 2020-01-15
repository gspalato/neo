using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;

using Arpa.Services;
using Arpa.Structures;
using Arpa.Utilities;


namespace Arpa.Commands
{
	public partial class Music : BaseCommandModule
	{
		[Command("queue")]
		public async Task ShowQueueAsync(CommandContext ctx)
		{
			MusicService musicService = ctx.Services.GetRequiredService<MusicService>();
			Player player = musicService.GetPlayer(ctx.Guild) as Player;

			if (player.connection == null)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription("Nothing's playing right now.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build());
				return;
			}

			if (player.queue.Count == 0)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription("The queue is empty.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build());
				return;
			}

			if ((player.queue.Count % 7) > 30)
			{
				// handle page excess
			}

			IEnumerable<IEnumerable<LavalinkTrack>> splitQueue = player.queue.Split(7);
			List<Page> pages = new List<Page>();

			int pageIndex = 1;
			foreach (IEnumerable<LavalinkTrack> split in splitQueue)
			{
				int trackIndex = 1;
				string content = "";
				foreach (LavalinkTrack track in split)
				{
					content += $"\n{trackIndex}. [{track.Title.TruncateAndEscape(30)}]({track.Uri})";
					trackIndex++;
				}

				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithTitle($"🎼 Queue | Page {pageIndex}/{splitQueue.Count()}")
					.WithDescription(content.Trim())
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				Page page = new Page(embed: embed);
				pages.Add(page);

				pageIndex++;
			}

			PaginationEmojis emojis = new PaginationEmojis
			{
				Left = DiscordEmoji.FromUnicode("◀"),
				Right = DiscordEmoji.FromUnicode("▶")
			};

			await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages.ToArray(), emojis);
		}
	}
}