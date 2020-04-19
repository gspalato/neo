using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Muon.Services;

namespace Muon.Kernel
{
	public interface IApp : IHostedService
	{
		new Task StartAsync(CancellationToken cancellationToken);
		new Task StopAsync(CancellationToken cancellationToken);
	}

	public class App : IApp, IHostedService
	{
		private IConfiguration _configuration;

		private DiscordSocketClient _client;

		/* Needed for instantiating. */
		private CommandHandlingService _commandHandler;
		private IEventService _eventService;

		public App(DiscordSocketClient client, IConfiguration configuration,
			CommandHandlingService commandHandler, IEventService eventService)
		{
			_configuration = configuration;
			_client = client;

			_commandHandler = commandHandler;
			_eventService = eventService;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			var socketClient = _client as DiscordSocketClient;

			await socketClient.LoginAsync(TokenType.Bot, _configuration.GetValue<string>("TOKEN"));
			await _client.StartAsync();

			await Task.Delay(-1);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _client.LogoutAsync();
		}
	}

}