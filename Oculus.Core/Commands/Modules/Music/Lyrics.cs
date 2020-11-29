using Oculus.Core.Structures.Attributes;
using Qmmands;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Oculus.Core.Commands.Modules.Music
{
	[Category(Category.Music)]
	[Description("But I can't help, giving you them lyrics..")]
	[Group("genius", "lyrics")]
	public class Lyrics : OculusModule
	{
		public LavaNode LavaNode { get; set; }

		[Command]
		[IgnoresExtraArguments]
		public async Task ExecuteAsync()
		{
			var range = Enumerable.Range(1900, 2000).ToArray();

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

			var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
			if (string.IsNullOrWhiteSpace(lyrics))
			{
				await SendDefaultEmbedAsync($"No lyrics found for {player.Track.Title}");
				return;
			}

			var splitLyrics = lyrics.Split('\n');
			var stringBuilder = new StringBuilder();
			foreach (var line in splitLyrics)
			{
				if (range.Contains(stringBuilder.Length))
				{
					await Context.ReplyAsync($"```{stringBuilder}```");
					stringBuilder.Clear();
				}
				else
				{
					stringBuilder.AppendLine(line);
				}
			}

			await Context.ReplyAsync($"```{stringBuilder}```");
		}
	}
}
