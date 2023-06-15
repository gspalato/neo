using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Oculus.Common.Utilities;
using Oculus.Common.Utilities.Extensions;
using Oculus.Core.Services;
using Oculus.Libraries.Interactivity;
using System.Diagnostics;

namespace Oculus.Core.Commands.Modules
{
    public class MiscellaneousModule : InteractionModuleBase
    {
        private InteractivityService InteractivityService { get; }
        private ILoggingService Logger { get; }
        private SnipeService SnipeService { get; }

        public MiscellaneousModule(InteractivityService interactivity, ILoggingService logger, SnipeService snipeService)
        {
            InteractivityService = interactivity;
            Logger = logger;
            SnipeService = snipeService;
        }

        [SlashCommand("ping", "Pong!")]
        public async Task PingAsync()
        {
            var sw = new Stopwatch();

            sw.Start();
            var response = await ReplyAsync("Measuring...").ConfigureAwait(false);
            sw.Stop();

            var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime.ToUniversalTime();

            var apiLatency = (Context.Client as DiscordSocketClient)!.Latency;
            var botLatency = sw.ElapsedMilliseconds - apiLatency;

            var embed = new EmbedBuilder()
                .WithTitle("🏓 Pong!")
                .WithDefaultColor()
                .AddField("API Latency", Format.Code($"{apiLatency}ms", ""), true)
                .AddField("Bot Latency", Format.Code($"{botLatency}ms", ""), true)
                .AddField("Uptime", Format.Code(uptime.ToHumanDuration(), ""), true);

            await response.ModifyAsync(x => x.Embed = embed.Build()).ConfigureAwait(false);
        }

        [SlashCommand("say", "Makes the bot say something")]
        public async Task SayAsync(string text)
        {
            await RespondAsync(text);
        }

        [SlashCommand("whois", "Shows information about that user.")]
        public async Task WhoIsAsync(IUser user)
        {
            var member = (SocketGuildUser) await Context.Guild.GetUserAsync(user.Id).ConfigureAwait(false);

            var escapedUsername = Format.Sanitize(user.Username);
            var escapedNickname = member.Nickname is not null
                ? $"({Format.Sanitize(member.Nickname)})"
                : "";

            var totalName = $"{escapedUsername} {escapedNickname}";

            var rolesList = from role in member.Roles
                            orderby role.Position descending
                            select Format.Code(Format.Sanitize(role.Name));

            var status = member.GetStatus();

            var embed = new EmbedBuilder()
                .WithAuthor($"{member.Username}#{member.Discriminator}", user.GetAvatarUrl())
                .WithDescription(status ?? "​")
                .WithColor(member.Id == Context.Client.CurrentUser.Id
                    ? new Color(0x0066ff)
                    : member.GetHighestColor(new Color(0, 0, 0))
                 )
                .AddField("Name", totalName, true)
                .AddField("Discriminator", $"#{member.Discriminator}", true)
                .AddField("ID", member.Id.ToString(), true)
                .AddField("Bot?", member.IsBot ? "Yes" : "No", true)
                .AddField("Status", member.Status.ToString(), true)
                .AddField("Joined", member.JoinedAt?.ToUniversalTime().ToString().Substring(1, 18), true)
                .WithThumbnailUrl(member.GetAvatarUrl());

            if (rolesList.Any())
                embed.AddField("Roles", string.Join(", ", rolesList));

            await RespondAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [SlashCommand("pfp", "Get an user's profile picture.")]
        public async Task ProfilePictureAsync(IUser user)
        {
            var embed = Utilities.CreateDefaultEmbed()
                .WithAuthor($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl())
                .WithImageUrl(user.GetAvatarUrl(ImageFormat.Png, 2048));

            await RespondAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [SlashCommand("snipe", "Get the latest deleted message!")]
        public async Task SnipeAsync(IGuildChannel? channel)
        {
            channel ??= Context.Channel as IGuildChannel;

            var snipedMessage = this.SnipeService.GetLatestMessageFromChannel(channel!.Id);

            if (snipedMessage is null)
            {
                await RespondAsync("Couldn't retrieve latest deleted message.", ephemeral: true).ConfigureAwait(false);
                return;
            }

            var embed = Utilities.CreateDefaultEmbed()
                .WithAuthor($"{snipedMessage.Author.Username}#{snipedMessage.Author.Discriminator}",
                    snipedMessage.Author.GetAvatarUrl())
                .WithDescription(snipedMessage.Content + (snipedMessage.Embeds.Any() ? "\n\n[Embed]" : ""))
                .WithFooter($"Deleted in #{channel.Name} at {snipedMessage.Timestamp.ToUniversalTime():HH:mm:ss}");

            await RespondAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}