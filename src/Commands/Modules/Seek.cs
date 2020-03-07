using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Muon.Core.Structures;
using Muon.Core.Utilities;
using Muon.Services;



namespace Muon.Commands
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
					.WithDefaultColor()
					.WithTimestamp(ctx.Message.Timestamp)
					.Build()).ConfigureAwait(false);
				return;
			}

			if (!player.isPlaying && player.queue.Count() == 0)
			{
				await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
					.WithDescription("Nothing is playing.")
					.WithDefaultColor()
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
				.WithDefaultColor()
				.WithTimestamp(ctx.Message.Timestamp)
				.Build()).ConfigureAwait(false);

			await player.connection.SeekAsync(correctedPosition);
		}
	}
}