using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Qmmands;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Axion.Core.Commands.Modules.Music
{
    [Category(Category.Music)]
    [Description("Skippity Skoppity, your tunes are now my property.")]
    [Group("skip")]
    public class Skip : AxionModule
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

            try
            {
                var currentTrack = await player.SkipAsync();

                await SendDefaultEmbedAsync(":notes: now playing",
                    $"**[{currentTrack.Title.TruncateAndSanitize()}]({currentTrack.Url})**");
            }
            catch (Exception exception)
            {
                await Context.ReplyAsync(exception.Message);
            }
        }
    }
}
