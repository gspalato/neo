using Axion.Core.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Axion
{
	public interface IApp : IHostedService
	{
		new Task StartAsync(CancellationToken cancellationToken);
		new Task StopAsync(CancellationToken cancellationToken);
	}

	public class App : IApp
	{
		private readonly IConfiguration _configuration;
		private readonly DiscordSocketClient _client;

		/* Needed for initializing. */
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
			await _client.LoginAsync(TokenType.Bot, _configuration.GetValue<string>("TOKEN"));
			await _client.StartAsync();

			await Task.Delay(-1, cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _client.LogoutAsync();
		}
	}

}