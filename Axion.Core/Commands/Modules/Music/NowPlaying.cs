using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Qmmands;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Utils = Axion.Core.Utilities;

namespace Axion.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("Check out what's playing right now.")]
	[Group("nowplaying", "np", "now-playing")]
	public sealed class NowPlaying : AxionModule
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

			var firstTracks = Utils::CommandUtilities.GetNearestTracksAsString(player.Queue);
			var slider = track.GenerateSlider();

			var totalTime = track.Duration.ToHumanDuration();
			var elapsedTime = track.Position.ToHumanDuration();
			var remainingTime = (track.Duration - track.Position).ToHumanDuration();

			var description = $"**[{track.Title.TruncateAndSanitize()}]({track.Url})**"
					+ $"\n`{remainingTime}` remaining.\n\n"
					+ firstTracks;

			var embed = CreateDefaultEmbed("🎶 Now Playing", description)
				.WithFooter($"{slider}  {elapsedTime} / {totalTime}");

			await SendEmbedAsync(embed: embed);
		}
	}
}