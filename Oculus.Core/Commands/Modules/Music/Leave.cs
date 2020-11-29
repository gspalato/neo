using Oculus.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Threading.Tasks;
using Victoria;

namespace Oculus.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Leave the voice channel. B(")]
	[Group("leave")]
	public class Leave : OculusModule
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

			var voiceChannel = Context.User.VoiceChannel ?? player.VoiceChannel;
			if (voiceChannel is null)
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			try
			{
				await LavaNode.LeaveAsync(voiceChannel);
			}
			catch (Exception exception)
			{
				await Context.ReplyAsync(exception.Message, null);
			}
		}
	}
}
