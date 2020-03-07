using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

using Muon.Services;
using Muon.Core.Structures;


namespace Muon.Commands
{
	public partial class Music : BaseCommandModule
	{
		[Command("play")]
		public async Task PlayAsync(CommandContext ctx, params string[] input)
		{
			DiscordChannel channel = ctx.Member.VoiceState.Channel;
			if (channel == null)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription($"You're not connected to a voice channel.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
			}

			string query = string.Join(" ", input);

			MusicService musicService = ctx.Services
				.GetRequiredService<MusicService>();

			IPlayer player = musicService.GetPlayer(ctx.Guild);
			LavalinkLoadResult result = await musicService.Resolve(query);
			if (result.LoadResultType == LavalinkLoadResultType.LoadFailed)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription($"An error occurred while loading the track.\n```{result.Exception.Message}```")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build());
			}

			IEnumerable<LavalinkTrack> tracks = result.Tracks;

			string displayName = "";
			string displayLink = "";

			if (result.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
			{
				foreach (LavalinkTrack track in tracks)
					player.Push(track);

				displayName = result.PlaylistInfo.Name;
			}
			else if (result.LoadResultType == LavalinkLoadResultType.TrackLoaded
					|| result.LoadResultType == LavalinkLoadResultType.SearchResult)
			{
				LavalinkTrack track = tracks.ElementAt(0);

				displayName = track.Title;
				displayLink = track.Uri.ToString();

				player.Push(track);
			}

			if ((player as Player).isPlaying)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription($"Added **[{displayName}]({displayLink})** to the queue.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
			}
			else
			{
				await player.Play(channel, ctx.Channel).ConfigureAwait(false);
			}
		}
	}
}