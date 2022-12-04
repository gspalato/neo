using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Player;
using Oculus.Common.Utilities.Extensions;
using Oculus.Kernel.Services;
using System.Text;

namespace Oculus.Kernel.Commands.Modules
{
    public class MusicModule : InteractionModuleBase
    {
        private readonly IMusicService _musicService;
        private readonly ILoggingService _logger;

        public MusicModule(IMusicService musicService, ILoggingService logger)
        {
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

            var player = GetPlayerAsync(voiceChannel);

            await RespondAsync($"Connected to {voiceChannel}.");
        }

        [SlashCommand("play", "Plays the selected song or adds it to the queue.")]
        public async Task PlayAsync(string search)
        {
            var user = Context.User as SocketGuildUser;
            var voiceChannel = user!.VoiceChannel;

            var player = await GetPlayerAsync(voiceChannel);

            var track = await _musicService.GetTrackAsync(search, Lavalink4NET.Rest.SearchMode.YouTube);

            _logger.Debug(track is not null ? $"Now playing: {track.Title.Truncate()}" : "No song matches found.");

            if (track is null)
            {
                await RespondAsync("No song matches found.", ephemeral: true);
                return;
            }

            var position = await player.PlayAsync(track);
            if (position is 0)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🎶 **Now Playing**")
                    .WithDescription($"[{track.Title.TruncateAndSanitize(60)}]({track.Uri})")
                    .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder()
                    .WithDescription($"**Queued** [{track.Title.TruncateAndSanitize(60)}]({track.Uri})")
                    .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: embed.Build());
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
                var emptyQueueEmbed = new EmbedBuilder()
                    .WithDescription($"There' no songs to play next.")
                    .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: emptyQueueEmbed.Build(), ephemeral: true);

                return;
            }

            await player.SkipAsync();
            var current = player.CurrentTrack!;
            var embed = new EmbedBuilder()
                    .WithTitle("🎶 **Now Playing**")
                    .WithDescription($"[{current.Title.TruncateAndSanitize(60)}]({current.Uri})")
                    .WithColor(new Color(0x2F3136));

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

                var emptyQueueEmbed = new EmbedBuilder()
                        .WithDescription($"⏸️  **Paused** [{current.Title.TruncateAndSanitize(40)}]({current.Uri})")
                        .WithColor(new Color(0x2F3136));

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

                var setVolumeEmbed = new EmbedBuilder()
                        .WithDescription($"{(volume is 0 ? "🔇" : "🔊")}  Set the volume to `{volume}%`")
                        .WithColor(new Color(0x2F3136));

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

                var stopEmbed = new EmbedBuilder()
                        .WithDescription($"⏹️  Stopped playing music.")
                        .WithColor(new Color(0x2F3136));

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

                var emptyQueueEmbed = new EmbedBuilder()
                        .WithDescription($"▶️  **Resumed** [{current.Title.TruncateAndSanitize(40)}]({current.Uri})")
                        .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: emptyQueueEmbed.Build());
            }
        }

        [SlashCommand("queue", "Displays the current queue.")]
        public async Task QueueAsync()
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;


        }

        [SlashCommand("seek", "Skip the song to a specific position.")]
        public async Task SeekAsync(int seconds)
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (player.State is PlayerState.Playing)
            {
                await player.SeekPositionAsync(TimeSpan.FromSeconds(seconds));

                var seekEmbed = new EmbedBuilder()
                    .WithDescription($"Now set to the moment `{player.Position.Position.ToHumanDuration()}.`")
                    .WithColor(new Color(0x2F3136));

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


            var embed = new EmbedBuilder()
                    .WithTitle("🎶 **Now Playing**")
                    .WithDescription(description.ToString())
                    .WithColor(new Color(0x2F3136))
                    .WithFooter(footer);

            await RespondAsync(embed: embed.Build());
        }


        private async Task<QueuedLavalinkPlayer?> PrecheckVoiceConditions(bool moveIfAvailable = true)
        {
            var user = Context.User as SocketGuildUser;
            var voiceChannel = user!.VoiceChannel;

            // Check if user is in a voice channel.
            if (voiceChannel is null)
            {
                var notConnectedEmbed = new EmbedBuilder()
                    .WithDescription($"You're not connected to any voice chat.")
                    .WithColor(new Color(0x2F3136));

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
                var notConnectedEmbed = new EmbedBuilder()
                    .WithDescription($"I'm not connected to any voice chat.")
                    .WithColor(new Color(0x2F3136));

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
                var notConnectedEmbed = new EmbedBuilder()
                    .WithDescription($"I'm already playing music in another voice chat.")
                    .WithColor(new Color(0x2F3136));

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
                var emptyQueueEmbed = new EmbedBuilder()
                    .WithDescription($"Nothing's currently playing.")
                    .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: emptyQueueEmbed.Build(), ephemeral: true);

                return null;
            }

            return player;
        }
    }
}
