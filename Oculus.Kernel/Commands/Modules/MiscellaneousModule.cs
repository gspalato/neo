using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Oculus.Common.Utilities.Extensions;
using Oculus.Kernel.Services;
using System.Diagnostics;

namespace Oculus.Kernel.Commands.Modules
{
    public class MiscellaneousModule : InteractionModuleBase
    {
        private readonly ILoggingService _logger;

        public MiscellaneousModule(ILoggingService logger)
        {
            _logger = logger;
        }

        [SlashCommand("ping", "Pong!")]
        public async Task PingAsync()
        {
            var sw = new Stopwatch();

            sw.Start();
            await RespondAsync("Measuring...");
            sw.Stop();

            var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime.ToUniversalTime();

            var apiLatency = (Context.Client as DiscordSocketClient)!.Latency;
            var botLatency = sw.ElapsedMilliseconds - apiLatency;

            var embed = new EmbedBuilder()
                .WithTitle("🏓 Pong!")
                .WithInfo()
                .AddField("API Latency", Format.Code($"{apiLatency}ms", ""), true)
                .AddField("Bot Latency", Format.Code($"{botLatency}ms", ""), true)
                .AddField("Uptime", Format.Code(uptime.ToHumanDuration(), ""), true);

            await FollowupAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [SlashCommand("say", "Makes the bot say something")]
        [Alias("echo")]
        public async Task SayAsync(string text)
        {
            await RespondAsync(text);
        }

        [SlashCommand("whois", "Shows information about that user.")]
        public async Task WhoIsAsync(IUser user)
        {
            var member = (SocketGuildUser)(await Context.Guild.GetUserAsync(user.Id));

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
                //.WithDescription(status)
                .WithColor(member.Id == Context.Client.CurrentUser.Id
                    ? new Color(0x0066ff)
                    : member.GetHighestColor(new Color()))
                .AddField("Name", totalName, true)
                .AddField("Discriminator", $"#{member.Discriminator}", true)
                .AddField("ID", member.Id.ToString(), true)
                .AddField("Bot?", member.IsBot ? "Yes" : "No", true)
                .AddField("Status", member.Status.ToString(), true)
                .AddField("Joined", member.JoinedAt?.ToUniversalTime().ToString().Substring(1, 18), true)
                .AddField("Roles", string.Join(", ", rolesList))
                .WithThumbnailUrl(member.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());
        }
    }
}