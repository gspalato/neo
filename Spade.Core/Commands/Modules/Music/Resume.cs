using Spade.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Spade.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Resume them tunes. B)")]
	[Group("resume")]
	public class Resume : SpadeModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task ResumeAsync()
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState is not PlayerState.Paused)
			{
				await SendDefaultEmbedAsync("The song's not paused!");
				return;
			}

			try
			{
				await player.ResumeAsync();
				await SendDefaultEmbedAsync("Resumed song.");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}
	}
}
