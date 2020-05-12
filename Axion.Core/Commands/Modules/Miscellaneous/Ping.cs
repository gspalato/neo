using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Miscellaneous
{
    [Category(Category.Miscellaneous)]
    [Description("Gives you the API latency.")]
    [Group("ping")]
    public class Ping : AxionModule
    {
        [Command]
        [IgnoresExtraArguments]
        public async Task ExecuteAsync()
        {
            var sw = new Stopwatch();

            sw.Start();
            var msg = await Context.ReplyAsync("Measuring...");
            sw.Stop();

            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();

            var embed = new EmbedBuilder()
                .WithTitle("🏓 Pong!")
                .WithInfo()
                .AddField("API Latency", Format.Code($"{Context.Client.Latency}ms", ""), true)
                .AddField("Bot Latency", Format.Code($"{sw.ElapsedMilliseconds}ms", ""), true)
                .AddField("Uptime", Format.Code(uptime.ToHumanDuration(), ""), true);

            await msg.ModifyAsync(props =>
            {
                props.Content = "";
                props.Embed = embed.Build();
            }).ConfigureAwait(false);
        }
    }
}
