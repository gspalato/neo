using Qmmands;
using System;
using System.Threading.Tasks;
using Axion.Core.Structures.Attributes;
using Victoria;

namespace Axion.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Set the beat's volume!")]
	[Group("volume", "vol", "v")]
	public class Volume : AxionModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task VolumeAsync(ushort volume)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			try
			{
				await player.UpdateVolumeAsync(volume);
				await SendDefaultEmbedAsync($"Volume set to `{volume}%`");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}

		[Command]
		[IgnoresExtraArguments]
		public async Task VolumeAsync(double volume) =>
			await VolumeAsync(Convert.ToUInt16(volume));
	}
}
