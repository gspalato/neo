using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

using Muon.Services;
using Muon.Core.Structures;
using Muon.Core.Utilities;


namespace Muon.Commands
{
	public partial class Music : BaseCommandModule
	{
		[Command("loop")]
		public async Task LoopAsync(CommandContext ctx)
		{
			MusicService musicService = ctx.Services.GetRequiredService<MusicService>();
			Player player = musicService.GetPlayer(ctx.Guild) as Player;

			if (!player.isPlaying && player.queue.Count() == 0)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription("Nothing is playing.")
					.WithDefaultColor()
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
				return;
			}

			if (player.isLooping)
			{
				player.isLooping = false;

				LavalinkTrack track = player.current;
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription($"Stopped looping.")
					.WithDefaultColor()
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
			}
			else
			{
				player.isLooping = true;

				LavalinkTrack track = player.current;
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription($"Looping **[{track.Title.TruncateAndEscape(20)}]({track.Uri})**")
					.WithDefaultColor()
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
			}
		}
	}
}