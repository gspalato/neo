using Discord;
using Discord.Interactions;
using Lavalink4NET.Player;
using Lavalink4NET;
using Discord.WebSocket;
using System.Text;
using Oculus.Common.Utilities.Extensions;
using Discord.Commands;
using Lavalink4NET.Logging;
using Oculus.Kernel.Services;

namespace Oculus.Kernel.Commands.Modules
{
    public class MusicModule : InteractionModuleBase
    {
        private readonly IAudioService _audioService;
        private readonly ILoggingService _logger;

        public MusicModule(IAudioService audioService, ILoggingService logger)
        {
            _audioService = audioService;
            _logger = logger;
        }

        private async Task<QueuedLavalinkPlayer> GetPlayerAsync(SocketVoiceChannel voiceChannel)
        {
            var player = _audioService.GetPlayer<QueuedLavalinkPlayer>(voiceChannel.Guild.Id)
                ?? await _audioService.JoinAsync<QueuedLavalinkPlayer>(voiceChannel.Guild.Id, voiceChannel.Id);

            return player;
        }

        [SlashCommand("join", "Makes Cassette join the user's voice channel.")]
        public async Task JoinAsync()
        {
            var user = Context.User as SocketGuildUser;
            var voiceChannel = user!.VoiceChannel;

            var player = GetPlayerAsync(voiceChannel);

            await RespondAsync($"Connected to {voiceChannel}.", null, false, true, null, null, null);
        }

        [SlashCommand("play", "Plays the selected song!")]
        public async Task PlayAsync(string queryOrLink)
        {
            var user = Context.User as SocketGuildUser;
            var voiceChannel = user!.VoiceChannel;

            var player = await GetPlayerAsync(voiceChannel);


            var track = await _audioService.GetTrackAsync(queryOrLink, Lavalink4NET.Rest.SearchMode.YouTube);

            _logger.Debug(track.Title ?? "No song matches found.");

            var position = await player.PlayAsync(track);
            if (position is 0)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🎶 **now playing**")
                    .WithDescription($"[{track.Title.TruncateAndSanitize(60)}]({track.Uri})")
                    .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder()
                    .WithDescription($"Queued [{track.Title.TruncateAndSanitize(60)}]({track.Uri})")
                    .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: embed.Build());
            }
        }

        [SlashCommand("skip", "Skips to the next song!")]
        public async Task SkipAsync()
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (!player.Queue.Any())
            {
                var emptyQueueEmbed = new EmbedBuilder()
                    .WithDescription($"There' no songs to play next.")
                    .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: emptyQueueEmbed.Build());

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
                        .WithDescription($"⏸️  Paused [{current.Title.TruncateAndSanitize(40)}]({current.Uri})")
                        .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: emptyQueueEmbed.Build());
            }
        }

        [SlashCommand("stop", "Stop playing all songs.")]
        public async Task StopAsync()
        {
            if (await PrecheckVoiceConditions() is not QueuedLavalinkPlayer player)
                return;

            if (player.State is PlayerState.Playing)
            {
                await player.StopAsync();

                var emptyQueueEmbed = new EmbedBuilder()
                        .WithDescription($"⏹️  Stopped playing music.")
                        .WithColor(new Color(0x2F3136));

                await RespondAsync(embed: emptyQueueEmbed.Build());
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
                        .WithDescription($"▶️  Resumed [{current.Title.TruncateAndSanitize(40)}]({current.Uri})")
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

        [SlashCommand("nowplaying", "Shows the currently playing song!")]
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
                .Append($" {current.Position.ToHumanDuration()} / {current.Duration.ToHumanDuration()}")
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

                await RespondAsync(embed: notConnectedEmbed.Build());

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

                await RespondAsync(embed: notConnectedEmbed.Build());

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

                await RespondAsync(embed: notConnectedEmbed.Build());

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

                await RespondAsync(embed: emptyQueueEmbed.Build());

                return null;
            }

            return player;
        }
    }
}
