using Axion.Core.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Axion.Core
{
	public class App : IHostedService
	{
		private readonly IConfiguration _configuration;
		private readonly DiscordSocketClient _client;

		private readonly ICommandHandlingService _commandHandler;
		private readonly IEventService _eventService;

		public App(DiscordSocketClient client, IConfiguration configuration,
			ICommandHandlingService commandHandler, IEventService eventService)
		{
			_configuration = configuration;
			_client = client;

			_commandHandler = commandHandler;
			_eventService = eventService;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_commandHandler.Start();
			_eventService.Listen();

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