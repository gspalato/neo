using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Arpa.Services;
using Arpa.Structures;


namespace Arpa.Commands
{
	public partial class Music : BaseCommandModule
	{
		[Command("volume")]
		[Aliases("vol")]
		public async Task VolumeAsync(CommandContext ctx, int volume)
		{
			MusicService musicService = ctx.Services.GetRequiredService<MusicService>();
			Player player = musicService.GetPlayer(ctx.Guild) as Player;

			DiscordChannel channel = ctx.Member.VoiceState.Channel;
			if (channel == null || channel != player.connection.Channel)
			{
				await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
					.WithDescription($"You're not connected to the voice channel.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp)
					.Build()).ConfigureAwait(false);
				return;
			}

			if (!player.isPlaying && player.queue.Count() == 0)
			{
				await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
					.WithDescription("Nothing is playing.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp)
					.Build()).ConfigureAwait(false);
				return;
			}

			if (volume > 100 || volume < 0)
			{
				await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
					.WithDescription($"Volume must be between 0 and 100.")
					.WithColor(new DiscordColor(0x2F3136))
					.WithTimestamp(ctx.Message.Timestamp)
					.Build()).ConfigureAwait(false);

				return;
			}

			await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
				.WithDescription($"Volume set to {volume}%")
				.WithColor(new DiscordColor(0x2F3136))
				.WithTimestamp(ctx.Message.Timestamp)
				.Build()).ConfigureAwait(false);

			player.connection.SetVolume(volume);
		}
	}
}