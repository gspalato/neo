using Axion.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Axion.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Pause them tunes. B(")]
	[Group("pause")]
	public class Pause : AxionModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task PauseAsync()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("I cannot pause when I'm not playing anything!");
				return;
			}

			try
			{
				await player.PauseAsync();

				await Context.ReplyAsync("Paused song.");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}
	}
}
