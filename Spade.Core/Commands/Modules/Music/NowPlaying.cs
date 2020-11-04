using Qmmands;
using Spade.Common.Extensions;
using Spade.Core.Structures.Attributes;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Spade.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Check out what's playing right now.")]
	[Group("nowplaying", "np", "now-playing")]
	public sealed class NowPlaying : SpadeModule
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

			if (player.PlayerState != PlayerState.Playing)
			{
				await SendDefaultEmbedAsync("Nothing's playing right now.");
				return;
			}

			var track = player.Track;

			var firstTracks = GetNearestTracksAsString(player.Queue);
			var slider = track.GenerateSlider();

			var totalTime = track.Duration.ToHumanDuration();
			var elapsedTime = track.Position.ToHumanDuration();
			var remainingTime = (track.Duration - track.Position).ToHumanDuration();

			var description = new StringBuilder();

			description.AppendLine($"**[{track.Title.TruncateAndSanitize()}]({track.Url})**");
			description.AppendLine($"`{remainingTime}` remaining.\n");
			description.Append(firstTracks);

			var embed = CreateDefaultEmbed("🎶 Now Playing", description.ToString())
				.WithFooter($"{slider}  {elapsedTime} / {totalTime}");

			await SendEmbedAsync(embed);
		}

		public string GetNearestTracksAsString(DefaultQueue<LavaTrack> queue)
		{
			if (queue.Count == 0)
				return "";

			var s = new StringBuilder();

			var elapsed = 0;
			foreach (var queueable in queue)
			{
				var track = (LavaTrack)queueable;
				if (elapsed >= 5)
					break;

				s.Append($"{++elapsed}. [{track.Title.TruncateAndSanitize()}]({track.Url})");
				s.Append("\n");
			}

			var remaining = queue.Count - elapsed;
			if (queue.Count > 5)
				s.Append($"and {remaining} more track{(remaining > 1 ? "s" : "")}...");

			return s.ToString();
		}
	}
}