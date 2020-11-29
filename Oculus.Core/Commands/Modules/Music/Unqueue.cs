using Oculus.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Oculus.Common.Extensions;
using Victoria;
using Victoria.Enums;

namespace Oculus.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Remove musics from the queue!")]
	[Group("unqueue")]
	public class Unqueue : OculusModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		public async Task ExecuteAsync(int pos)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (!(new [] { PlayerState.Playing, PlayerState.Paused }).Contains(player.PlayerState))
			{
				await SendDefaultEmbedAsync("Nothing's playing right now.");
				return;
			}
			
			if (player.Queue.ElementAt(pos) is null)
			{
				await SendDefaultEmbedAsync($"There isn't any track at position `{pos}`.");
				return;
			}

			var dequeued = player.Queue.RemoveAt(pos) as LavaTrack;

			await SendDefaultEmbedAsync(
				$"Removed track **[{dequeued?.Title.TruncateAndSanitize()}]({dequeued?.Url})** at position `{pos}`.");
		}

		[Command]
		public async Task ExecuteAsync(int start, int end)
		{
			if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendDefaultEmbedAsync("I'm not connected to a voice channel.");
				return;
			}

			if (!(new [] { PlayerState.Playing, PlayerState.Paused }).Contains(player.PlayerState))
			{
				await SendDefaultEmbedAsync("Nothing's playing right now.");
				return;
			}

			var queueCount = player.Queue.Count();
			if (start < queueCount || end > queueCount)
			{
				await SendDefaultEmbedAsync($"Invalid range.");
				return;
			}

			player.Queue.RemoveRange(start, end);

			await SendDefaultEmbedAsync($"Removed {end - start} tracks.");
		}
	}
}