using Spade.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;
using Victoria;

namespace Spade.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Group("join")]
	public class Join : SpadeModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task ExecuteAsync()
		{
			if (LavaNode.HasPlayer(Context.Guild))
			{
				await Context.ReplyAsync("I'm already connected to a voice channel!");
				return;
			}

			var voiceState = Context.User as IVoiceState;

			if (voiceState?.VoiceChannel is null)
			{
				await Context.ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			try
			{
				await LavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel);
				await Context.ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message);
			}
		}
	}
}
