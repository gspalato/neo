using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;

using Muon.Services;

namespace Muon.Kernel
{
	public interface IApp
	{
		public Task StartAsync(CancellationToken cancellationToken);
		public Task StopAsync(CancellationToken cancellationToken);
	}

	public class App : IApp, IHostedService
	{
		private DiscordClient _client;
		private ILogger<App> _logger;
		
		private IDatabaseService _databaseService;
		private ICommandService _commandService;
		private IMusicService _musicService;

		public App(DiscordClient client, ILogger<App> logger,
			IDatabaseService database, ICommandService commands, IMusicService music)
		{
			_client = client;
			_logger = logger;

			_databaseService = database;
			_commandService = commands;
			_musicService = music;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			this.RegisterEvents();

			await _client.ConnectAsync();

			await Task.Delay(-1);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _client.DisconnectAsync();
		}

		private void RegisterEvents()
		{
			_client.Ready += async (ReadyEventArgs args) =>
			{
				this.InitializeServices();
				_logger.LogInformation("Ready!");

				await Task.Run(() => true);
			};

			_client.ClientErrored += async (ClientErrorEventArgs args) =>
			{
				_logger.LogError(args.Exception, "{Exception}");

				await Task.Run(() => true);
			};
		}

		private void InitializeServices()
		{
			_databaseService.Initialize();
			_commandService.InstallCommandsAsync();
			_ = Task.Run(() => _musicService.Initialize(_client.UseLavalink()));
		}
	}

}