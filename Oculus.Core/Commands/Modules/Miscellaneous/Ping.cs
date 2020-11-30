using Discord;
using Oculus.Common.Extensions;
using Oculus.Core.Structures.Attributes;
using Qmmands;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Miscellaneous
{
    [Category(Category.Miscellaneous)]
    [Description("Gives you the API latency.")]
    [Group("ping")]
    public class Ping : OculusModule
    {
        [Command]
        [IgnoresExtraArguments]
        public async Task ExecuteAsync()
        {
            var sw = new Stopwatch();

            sw.Start();
            var msg = await Context.ReplyAsync("Measuring...");
            sw.Stop();

            var uptime = Context.Now - Process.GetCurrentProcess().StartTime.ToUniversalTime();

            var apiLatency = Context.Client.Latency;
            var botLatency = sw.ElapsedMilliseconds - apiLatency;

            var embed = new EmbedBuilder()
                .WithTitle("🏓 pong!")
                .WithInfo()
                .AddField("API Latency", Format.Code($"{apiLatency}ms", ""), true)
                .AddField("Bot Latency", Format.Code($"{botLatency}ms", ""), true)
                .AddField("Uptime", Format.Code(uptime.ToHumanDuration(), ""), true);

            await msg.ModifyAsync(props =>
            {
                props.Content = "";
                props.Embed = embed.Build();
            }).ConfigureAwait(false);
        }
    }
}
