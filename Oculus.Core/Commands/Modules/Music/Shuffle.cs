using Oculus.Core.Structures.Attributes;
using Qmmands;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Oculus.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Shuffle those tunes! B)")]
	[Group("shuffle")]
	public class Shuffle : OculusModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task ShuffleAsync()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState is not PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("Nothing's playing right now.");
				return;
			}

			player.Queue.Shuffle();

			await SendDefaultEmbedAsync("Shuffled queue.");
		}
	}
}
