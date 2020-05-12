using Axion.Core.Extensions;
using Axion.Core.Structures.Interactivity;
using Discord;
using Qmmands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axion.Core.Structures.Attributes;
using Victoria;
using Victoria.Enums;
using Victoria.Interfaces;

namespace Axion.Core.Commands.Modules.Music
{
    [Category(Category.Music)]
    [Description("Check out what tunes are going to play next!")]
    [Group("queue")]
    public class Queue : AxionModule
    {
        public LavaNode LavaNode { get; set; }

        [Command]
        [IgnoresExtraArguments]
        public async Task QueueAsync()
        {
            try
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

                var pagedBuilder = new PaginatedMessageBuilder()
                    .WithDefaultButtons()
                    .WithResponsible(Context.User)
                    .WithTemplate(() =>
                        new EmbedBuilder()
                            .WithDefaultColor());

                var queue = player.Queue.Items.Chunk(7);
                var chunks = queue as IQueueable[][] ?? queue.ToArray();

                if (!chunks.Any())
                {
                    await SendDefaultEmbedAsync("There are no tracks next.");
                    return;
                }

                var totalTrackNumber = 0;
                for (var chunkNumber = 0; chunkNumber < chunks.Count(); chunkNumber++)
                {
                    var chunk = chunks.ElementAt(chunkNumber);
                    var description = new StringBuilder();

                    if (chunk is null)
                        break;

                    for (var trackNumber = 0; trackNumber < chunk.Count(); trackNumber++)
                    {
                        if (!(chunk.ElementAt(trackNumber) is LavaTrack track))
                            return;

                        description.Append($"{++totalTrackNumber}. [**{track.Title.TruncateAndSanitize()}**]({track.Url})\n");
                    }

                    pagedBuilder.AddPage(template =>
                    {
                        template
                            .WithTitle($":musical_score: queue · page {chunkNumber} of {chunks.Count()}")
                            .WithDescription(description.ToString());
                    });
                }

                var pagedMessage = pagedBuilder.Build(Context.Client);
                await pagedMessage.Send(Context.Channel);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n{e.StackTrace}");
            }
        }
    }
}
