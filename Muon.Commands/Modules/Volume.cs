using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Muon.Kernel.Structures;
using Muon.Kernel.Utilities;
using Muon.Services;


namespace Muon.Commands
{
	public partial class Music : BaseCommandModule
	{
		[Command("volume")]
		[Aliases("vol")]
		public async Task VolumeAsync(CommandContext ctx, int volume)
		{
			IMusicService musicService = ctx.Services.GetRequiredService<IMusicService>();
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

			if (volume > 100 || volume < 0)
			{
				await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
					.WithDescription($"Volume must be between 0 and 100.")
					.WithDefaultColor()
					.WithTimestamp(ctx.Message.Timestamp)
					.Build()).ConfigureAwait(false);

				return;
			}

			await player.connection.SetVolumeAsync(volume);

			await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
				.WithDescription($"Volume set to {volume}%")
				.WithDefaultColor()
				.WithTimestamp(ctx.Message.Timestamp)
				.Build()).ConfigureAwait(false);
		}
	}
}