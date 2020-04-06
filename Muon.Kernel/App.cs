using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
		private DiscordClient client;
		private IServiceProvider services;

		public App(IServiceProvider services, DiscordClient client, IConfiguration configuration)
		{
			this.client = client;
			this.services = services;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			this.RegisterEvents();

			await client.ConnectAsync();

			await Task.Delay(-1);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await client.DisconnectAsync();
		}

		private void RegisterEvents()
		{
			client.Ready += async (ReadyEventArgs args) =>
			{
				await this.InitializeServices();
				await services.GetRequiredService<LoggingService>().LogAsync("Ready!");
			};

			client.ClientErrored += (ClientErrorEventArgs args) =>
				services.GetRequiredService<LoggingService>().LogAsync(args.Exception.ToString());
		}

		private async Task InitializeServices()
		{
			services.GetRequiredService<DatabaseService>().Initialize();
			services.GetRequiredService<CommandService>().InstallCommandsAsync();
			await services.GetRequiredService<MusicService>().Initialize(client.UseLavalink());
		}
	}

}