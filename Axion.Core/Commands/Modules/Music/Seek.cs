using Axion.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Axion.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Skip to a certain time in the song.")]
	[Group("seek")]
	public class Seek : AxionModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task SeekAsync(TimeSpan timeSpan)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("I'm not playing anything!.");
				return;
			}

			try
			{
				await player.SeekAsync(timeSpan);

				await SendDefaultEmbedAsync($"Seeked to `{timeSpan.TotalSeconds}s`");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[IgnoresExtraArguments]
		public async Task SeekAsync(double seconds) =>
			await SeekAsync(TimeSpan.FromSeconds(seconds));
	}
}
