using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

using Arpa;
using Arpa.Services;
using Arpa.Structures;


namespace Arpa.Commands
{
	[Description("Drop the beat.")]
	public class Music : BaseCommandModule
	{
		[Command("play")]
		public async Task PlayAsync(CommandContext ctx, params string[] input)
		{
			DiscordChannel channel = ctx.Member.VoiceState.Channel;
			if (channel == null)
			{
				// handle not connected to vc
			}

			string text = string.Join(" ", input);

			MusicService musicService = ctx.Services
				.GetRequiredService<MusicService>();

			IPlayer player = musicService.GetPlayer(ctx.Guild);
			LavalinkLoadResult result = await musicService.Resolve(text);
			LavalinkTrack track = result.Tracks.First();

			player.Push(track);

			if ((player as Player).isPlaying)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription($"Added [{track.Title}]({track.Uri}) to the queue.")
					.WithColor(new DiscordColor(0xAA0099))
					.WithTimestamp(new DateTimeOffset());

				await ctx.RespondAsync(embed: embed.Build());
			}
			else
			{
				await player.Play(channel);

				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithTitle("🎶 Now Playing")
					.WithDescription($"[{track.Title}]({track.Uri})")
					.WithColor(new DiscordColor(0xAA0099))
					.WithTimestamp(new DateTimeOffset());

				await ctx.RespondAsync(embed: embed.Build());
			}
		}
	}
}