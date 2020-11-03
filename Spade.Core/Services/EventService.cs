using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Victoria;

namespace Spade.Core.Services
{
	public interface IEventService
	{
		public void Listen();
	}

	public class EventService : IEventService
	{
		private DiscordSocketClient Client { get; }
		private LavaNode LavaNode { get; }
		private ILoggingService Logger { get; }

		public EventService(DiscordSocketClient client,
			/*LavaNode lavaNode,*/ ILoggingService logger)
		{
			Client = client;
			//LavaNode = lavaNode;
			Logger = logger;
		}

		public void Listen()
		{
			Client.Ready += OnReadyAsync;
			Client.Log += Log;
		}

		private async Task OnReadyAsync()
		{
			//_ = LavaNode.ConnectAsync();

			await Client.SetGameAsync($"{Client.Guilds.Count} guilds.", type: ActivityType.Watching);
		}

		private Task Log(LogMessage log)
		{
			Logger.Log(log.Severity, log.Message, log.Exception, "dnet");

			return Task.CompletedTask;
		}
	}
}
