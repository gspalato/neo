using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

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
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription($"You're not connected to a voice channel.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build());
			}

			string query = string.Join(" ", input);

			MusicService musicService = ctx.Services
				.GetRequiredService<MusicService>();

			IPlayer player = musicService.GetPlayer(ctx.Guild);
			LavalinkLoadResult result = await musicService.Resolve(query);
			if (!result.Exception.Message.Equals(null))
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
			else if (result.LoadResultType == LavalinkLoadResultType.TrackLoaded)
			{
				LavalinkTrack track = tracks.ElementAt(0);

				displayName = track.Title;
				displayLink = track.Uri.ToString();

				player.Push(track);
			}

			if ((player as Player).isPlaying)
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithDescription($"Added [{displayName}]({displayLink}) to the queue.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build());
			}
			else
			{
				await player.Play(channel, ctx.Channel);

				DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
					.WithTitle("🎶 Now Playing")
					.WithDescription($"[{displayName}]({displayLink})")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp);

				await ctx.RespondAsync(embed: embed.Build());
			}
		}

		private bool IsValidLink(string link)
		{
			IPHostEntry host = Dns.GetHostEntry(link);
			return (host.HostName == "http://youtube.com") || (host.HostName == "http://youtu.be");
		}
	}
}