using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria;

namespace Axion.Services
{
    public interface IEventService
    {
        public Task OnReadyAsync();
    }

    public class EventService : IEventService
    {
        private DiscordSocketClient _client { get; }
        private LavaNode _lavaNode { get; set; }
        private ILoggingService _logger { get; }

        public EventService(DiscordSocketClient client,
            LavaNode lavaNode, ILoggingService logger)
        {
            _client = client;
            _lavaNode = lavaNode;
            _logger = logger;

            _client.Ready += OnReadyAsync;
            _client.Log += Log;
        }

        public async Task OnReadyAsync()
        {
            _ = _lavaNode.ConnectAsync();

            await _client.SetGameAsync($"{_client.Guilds.Count()} guilds.", type: ActivityType.Watching);
        }

        public Task Log(LogMessage log)
        {
            _logger.Log(log.Severity, log.Message, log.Exception, "dnet");

            return Task.CompletedTask;
        }
    }
}
