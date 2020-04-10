using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria;

namespace Muon.Services
{
    public interface IEventService
    {
        public Task OnReadyAsync();
    }

    public class EventService : IEventService
    {
        private IDatabaseService _databaseService { get; }
        private DiscordSocketClient _client { get; }
        private LavaNode _lavaNode { get; set; }

        public EventService(IDatabaseService databaseService,
            DiscordSocketClient client, LavaNode lavaNode)
        {
            _databaseService = databaseService;
            _client = client;
            _lavaNode = lavaNode;

            _client.Ready += OnReadyAsync;
            _client.Log += Log;
        }

        public Task OnReadyAsync()
        {
            _databaseService.Initialize();
            _ = _lavaNode.ConnectAsync();

            return Task.CompletedTask;
        }

        public Task Log(LogMessage log)
        {
            Console.WriteLine(log.Message ?? "" + "\n" + log.Exception ?? "");

            return Task.CompletedTask;
        }
    }
}
