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
		[Command("seek")]
		public async Task SeekAsync(CommandContext ctx, int position)
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

			TimeSpan span = TimeSpan.FromSeconds(position);
			TimeSpan correctedPosition = (position < 0)
				? TimeSpan.FromSeconds(0)
				: (span > player.current.Length)
					? player.current.Length
					: span;

			string readableSpan = this.ToHumanReadableTimeSpan((long)correctedPosition.TotalMilliseconds);

			await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
				.WithDescription($"Seeking to `{readableSpan}`.")
				.WithColor(new DiscordColor(0x2F3136))
				.WithTimestamp(ctx.Message.Timestamp)
				.Build()).ConfigureAwait(false);

			player.connection.Seek(correctedPosition);
		}
	}
}