using Spade.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Spade.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Stop them tunes. B(")]
	[Group("stop")]
	public class Stop : SpadeModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task ExecuteAsync()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState is PlayerState.Stopped)
			{
				await SendDefaultEmbedAsync("The song's already stopped!");
				return;
			}

			try
			{
				await player.StopAsync();
				await SendDefaultEmbedAsync("Stopped playing.");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message, null);
			}
		}
	}
}
