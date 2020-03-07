using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Muon.Services;
using Muon.Core.Structures;


namespace Muon.Commands
{
	public partial class Music : BaseCommandModule
	{
		[Command("skip")]
		public async Task SkipAsync(CommandContext ctx)
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

			await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
				.WithDescription("Skipping current song...")
				.WithColor(new DiscordColor(0x2F3136))
				.WithTimestamp(ctx.Message.Timestamp)
				.Build()).ConfigureAwait(false);

			player.Skip();
		}
	}
}