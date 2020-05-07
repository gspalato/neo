using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria;

namespace Axion.Core.Services
{
    public interface IEventService
    {
        public Task OnReadyAsync();
    }

    public class EventService : IEventService
    {
        private DiscordSocketClient Client { get; }
        private LavaNode LavaNode { get; }
        private ILoggingService Logger { get; }

        public EventService(DiscordSocketClient client,
            LavaNode lavaNode, ILoggingService logger)
        {
            Client = client;
            LavaNode = lavaNode;
            Logger = logger;

            Client.Ready += OnReadyAsync;
            Client.Log += Log;
        }

        public async Task OnReadyAsync()
        {
            _ = LavaNode.ConnectAsync();

            await Client.SetGameAsync($"{Client.Guilds.Count()} guilds.", type: ActivityType.Watching);
        }

        public Task Log(LogMessage log)
        {
            Logger.Log(log.Severity, log.Message, log.Exception, "dnet");

            return Task.CompletedTask;
        }
    }
}
