using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Oculus.Common.Utilities;
using Oculus.Common.Utilities.Extensions;
using Oculus.Core.Services;
using System.Text;
using System.Text.RegularExpressions;
using Oculus.Libraries.Interactivity;
using Oculus.Libraries.Interactivity.Structures.Builders;
using Oculus.Libraries.Interactivity.Structures.Contexts;

namespace Oculus.Core.Commands.Modules
{
    public class MusicModule : InteractionModuleBase
    {
        private readonly InteractivityService _interactivityService;
        private readonly IMusicService _musicService;
        private readonly ILoggingService _logger;

        public MusicModule(InteractivityService interactivityService, IMusicService musicService,
            ILoggingService logger)
        {
            _interactivityService = interactivityService;
            _musicService = musicService;
            _logger = logger;
        }

        public override void BeforeExecute(ICommandInfo command)
        {
            if (!_musicService.IsInitialized)
            {
                _logger.Error($"Failed to execute command '{command.Name}': MusicService was not initialized properly.");
                RespondAsync("Oh shi-! There was a problem when trying to do this. Try again later.", ephemeral: true);
                return;
            }

            base.BeforeExecute(command);
        }

        private async Task<QueuedLavalinkPlayer> GetPlayerAsync(SocketVoiceChannel voiceChannel)
        {
            var player = _musicService.GetPlayer<QueuedLavalinkPlayer>(voiceChannel.Guild.Id)
                ?? await _musicService.JoinAsync<QueuedLavalinkPlayer>(voiceChannel.Guild.Id, voiceChannel.Id);

            return player;
        }

        [SlashCommand("join", "Makes Cassette join the user's voice channel.")]
        public async Task JoinAsync()
        {
            var user = Context.User as SocketGuildUser;
            var voiceChannel = user!.VoiceChannel;

            if (voiceChannel is null)
            {
                var notConnectedEmbed = Utilities.CreateDefaultEmbed(
                    description: $"You're not connected to any voice chat.");
                await RespondAsync(embed: notConnectedEmbed.Build(), ephemeral: true);
                return;
            }

            await GetPlayerAsync(voiceChannel);

            await RespondAsync($"Connected to {voiceChannel}.");
        }

        [SlashCommand("play", "Plays the selected song or adds it to the queue.")]
        public async Task PlayAsync(string search)
        {                
            var user = Context.User as SocketGuildUser;
            var voiceChannel = user!.VoiceChannel;

            if (voiceChannel is null)
            {
                var notConnectedEmbed = Utilities.CreateDefaultEmbed(
                    description: $"You're not connected to any voice chat.");
                await RespondAsync(embed: notConnectedEmbed.Build(), ephemeral: true);
                return;
            }

            var player = await GetPlayerAsync(voiceChannel);
            
            async Task HandleSingleTrack(LavalinkTrack? track)
            {
                _logger.Debug(track is not null ? $"Now playing: {track.Title.Truncate()}" : "No song matches found.");

                if (track is null)
                {
                    await RespondAsync("No song matches found.", ephemeral: true);
                    return;
                }

                var position = await player.PlayAsync(track);
                if (position is 0)
                {
                    var embed = Utilities.CreateDefaultEmbed("🎶 **Now Playing**",
                        $"[{track.Title.TruncateAndSanitize(60)}]({track.Uri})");
                    await RespondAsync(embed: embed.Build());
                }
                else
                {
                    var embed = Utilities.CreateDefaultEmbed(
                        description: $"**Queued** [{track.Title.TruncateAndSanitize(60)}]({track.Uri})");
                    await RespondAsync(embed: embed.Build());
                }
            }

            async Task HandlePlaylist(TrackLoadResponsePayload response)
            {
                var firstTrack = response.Tracks!.First();

                _logger.Debug($"Now playing: {firstTrack.Title.Truncate()} and queueing {response.Tracks!.Length - 1} more songs.");

                if (response.Tracks.Length == 0)
                {
                    await RespondAsync("No song matches found.", ephemeral: true);
                    return;
                }

                int? firstPosition = null;
                foreach (LavalinkTrack track in response.Tracks)
                {
                    var position = await player.PlayAsync(track);
                    firstPosition = (firstPosition is null) ? position : firstPosition;
                }

                if (firstPosition is 0)
                {
                    var embed = Utilities.CreateDefaultEmbed("🎶 **Now Playing**",
                        $"[{firstTrack.Title.TruncateAndSanitize(60)}]({firstTrack.Uri})\n and queued {response.Tracks.Length - 1} more songs.");
                    await RespondAsync(embed: embed.Build());
                }
                else
                {
                    var embed = Utilities.CreateDefaultEmbed(
                        description: $"**Queued** [{firstTrack.Title.TruncateAndSanitize(60)}]({firstTrack.Uri}) and {response.Tracks.Length - 1} more songs.");
                    await RespondAsync(embed: embed.Build());
                }
            }

            var playlistLinkRegex = new Regex(@"^.*(youtu.be\/|list=)([^#\&\?]*).*");
            if (playlistLinkRegex.Match(search).Success)
            {
                var response = await _musicService.LoadTracksAsync(search, SearchMode.YouTube);

                switch (response.LoadType)
                {
                    case TrackLoadType.TrackLoaded:
                        await HandleSingleTrack(response.Tracks!.First());
                        break;

                    case TrackLoadType.PlaylistLoaded:
                        await HandlePlaylist(response);
                        break;

                    case TrackLoadType.NoMatches:
                        await RespondAsync("No song matches found.", ephemeral: true);
                        break;
                }
            }
            else
            {
                var track = await _musicService.GetTrackAsync(search, SearchMode.YouTube);
                await HandleSingleTrack(track);
            }
        }

        [SlashCommand("skip", "Skips to the next song.")]
        [Discord.Interactions.RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SkipAsync()
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (!player.Queue.Any())
            {
                var emptyQueueEmbed = Utilities.CreateDefaultEmbed(
                    description: $"There' no songs to play next.");;
                await RespondAsync(embed: emptyQueueEmbed.Build(), ephemeral: true);

                return;
            }

            await player.SkipAsync();
            var current = player.CurrentTrack!;
            var embed = Utilities.CreateDefaultEmbed("🎶 **Now Playing**",
                    $"[{current.Title.TruncateAndSanitize(60)}]({current.Uri})");
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("pause", "Pauses the current song.")]
        public async Task PauseAsync()
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (player.State is PlayerState.Playing)
            {
                var current = player.CurrentTrack!;
                await player.PauseAsync();

                var emptyQueueEmbed = Utilities.CreateDefaultEmbed(
                    description: $"⏸️  **Paused** [{current.Title.TruncateAndSanitize(40)}]({current.Uri})");
                await RespondAsync(embed: emptyQueueEmbed.Build());
            }
        }

        [SlashCommand("volume", "Sets the bot's volume.")]
        public async Task VolumeAsync([MinValue(0)][MaxValue(100)] int volume = 100)
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (player.State is PlayerState.Playing)
            {
                await player.SetVolumeAsync(volume / 100f, true);

                var setVolumeEmbed = Utilities.CreateDefaultEmbed(
                    description: $"{(volume is 0 ? "🔇" : "🔊")}  Set the volume to `{volume}%`");
                await RespondAsync(embed: setVolumeEmbed.Build());
            }
        }

        [SlashCommand("stop", "Stops playing all songs.")]
        public async Task StopAsync()
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (player.State is PlayerState.Playing)
            {
                await player.StopAsync();

                var stopEmbed = Utilities.CreateDefaultEmbed(
                    description: $"⏹️  Stopped playing music.");
                await RespondAsync(embed: stopEmbed.Build());
            }
        }

        [SlashCommand("resume", "Resumes the song.")]
        public async Task ResumeAsync()
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (player.State is PlayerState.Paused)
            {
                var current = player.CurrentTrack!;
                await player.ResumeAsync();

                var emptyQueueEmbed = Utilities.CreateDefaultEmbed(
                    description: $"▶️  **Resumed** [{current.Title.TruncateAndSanitize(40)}]({current.Uri})");
                await RespondAsync(embed: emptyQueueEmbed.Build());
            }
        }

        [SlashCommand("queue", "Displays the current queue.")]
        public async Task QueueAsync()
        {
        try
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (!player.Queue.Any())
            {
                var emptyQueueEmbed = Utilities.CreateDefaultEmbed(description: "The queue is empty.");
                await RespondAsync(embed: emptyQueueEmbed.Build(), ephemeral: true);
                return;
            }

            LavalinkTrack[][] queueChunks = Enumerable.Chunk(player.Queue, 7).ToArray();

            var pages = new List<Embed>();

            int pageCount = 0;
            foreach (var chunk in queueChunks)
            {
                var page = new EmbedBuilder()
                    .WithTitle($"🎼  Queue | Page {pageCount + 1} / {queueChunks.Count()}")
                    .WithColor(new Color(47, 49, 54));

                var description = new StringBuilder();

                int trackCount = 0;
                foreach (var track in chunk)
                {
                    description.AppendLine($"**{trackCount + 1}.** [{track.Title.TruncateAndSanitize(60)}]({track.Uri})");
                    trackCount++;
                }

                page.WithDescription(description.ToString());

                pages.Add(page.Build());

                pageCount++;
            }

            var paginationBuilder = new PaginationBuilder()
                .WithPages(pages)
                .WithDefaultButtons()
                .WithUser(Context.User);

            var interactivityBuilder = new InteractivityBuilder()
                .WithPagination(paginationBuilder);

            var (firstPage, components) = _interactivityService.UseInteractivity(interactivityBuilder);

            await RespondAsync(embed: firstPage, components: components.Build());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
        }

        [SlashCommand("seek", "Skip the song to a specific position.")]
        public async Task SeekAsync(int seconds)
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (player.State is PlayerState.Playing)
            {
                await player.SeekPositionAsync(TimeSpan.FromSeconds(seconds));

                var seekEmbed = Utilities.CreateDefaultEmbed(
                    description: $"Now set to the moment `{player.Position.Position.ToHumanDuration()}`.");
                await RespondAsync(embed: seekEmbed.Build());
            }
        }

        [SlashCommand("nowplaying", "Shows the currently playing song.")]
        [Alias("np")]
        public async Task NowPlayingAsync()
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            var current = player.CurrentTrack;
            var remaining = current!.Duration - player.Position.Position;

            var sliderPosition = Math.Floor(20 * player.Position.Position.TotalSeconds / current.Duration.TotalSeconds);

            var slider = new StringBuilder("▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬")
                .Insert(index: (int)sliderPosition, value: "🔵")
                .ToString();

            var footer = new StringBuilder(slider)
                .Append($"   {player.Position.Position.ToHumanDuration()} / {current.Duration.ToHumanDuration()}")
                .ToString();

            var description = new StringBuilder()
                .AppendLine($"**[{current.Title.TruncateAndSanitize(60)}]({current.Uri})**")
                .AppendLine($"`{remaining.ToHumanDuration()}` remaining.")
                .AppendLine();

            var next = player.Queue.Take(5);
            var count = 1;
            foreach (var track in next)
            {
                description.AppendLine($"{count}. [{track.Title.TruncateAndSanitize(20)}]({track.Uri})");
                count++;
            }
            if (player.Queue.Count > 5)
            {
                description.AppendLine("...");
            }


            var embed = Utilities.CreateDefaultEmbed("🎶 **Now Playing**",
                description.ToString(), footer: footer);
            await RespondAsync(embed: embed.Build());
        }


        private async Task<QueuedLavalinkPlayer?> PrecheckVoiceConditions(bool moveIfAvailable = true)
        {
            var user = Context.User as SocketGuildUser;
            var voiceChannel = user!.VoiceChannel;

            // Check if user is in a voice channel.
            if (voiceChannel is null)
            {
                var notConnectedEmbed = Utilities.CreateDefaultEmbed(
                    description: $"You're not connected to any voice chat.");
                await RespondAsync(embed: notConnectedEmbed.Build(), ephemeral: true);

                return null;
            }

            /*
            var player = await GetPlayerAsync(voiceChannel);
            if (player is null || player.State is PlayerState.NotConnected)
            {
                var emptyQueueEmbed = new EmbedBuilder()
                    .WithDescription($"I'm not connected to any chat.")
                    .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: emptyQueueEmbed.Build());

                return null;
            }
            */

            var botMember = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);

            // Check if the bot is not in a voice channel.
            if (botMember.VoiceChannel is null)
            {
                var notConnectedEmbed = Utilities.CreateDefaultEmbed(
                    description: $"I'm not connected to any voice chat.");
                await RespondAsync(embed: notConnectedEmbed.Build(), ephemeral: true);

                return null;
            }

            var firstPlayer = await GetPlayerAsync((SocketVoiceChannel)botMember.VoiceChannel);
            QueuedLavalinkPlayer? player = firstPlayer;

            var inSameVoiceChannel = voiceChannel.Id == botMember.VoiceChannel.Id;

            // In case the bot is connected,
            // Check if the bot is in another voice channel.
            // If it is and it's already playing something, alert.
            if (!inSameVoiceChannel && firstPlayer.State is PlayerState.Playing)
            {
                var notConnectedEmbed = Utilities.CreateDefaultEmbed(
                    description: $"I'm already playing music in another voice chat.");

                await RespondAsync(embed: notConnectedEmbed.Build(), ephemeral: true);

                return null;

            }
            // If it is but it's not doing anything (and moveIfAvailable is true),
            // Move to the user's voice channel.
            else if (!inSameVoiceChannel && firstPlayer.State is not PlayerState.Playing && moveIfAvailable)
            {
                player = await GetPlayerAsync(voiceChannel);
            }

            // Check if the bot is already playing something.
            if (player.State is PlayerState.NotPlaying)
            {
                var emptyQueueEmbed = Utilities.CreateDefaultEmbed(
                    description: $"Nothing's currently playing.");
                await RespondAsync(embed: emptyQueueEmbed.Build(), ephemeral: true);

                return null;
            }

            return player;
        }
    }
}
