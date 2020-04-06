using System;
using System.IO;
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

/*
namespace Muon.Kernel
{
	public interface IApp
	{
		Task MainAsync(string[] argv);
	}

	public class App : IApp
	{
		public DiscordClient Client;
		public IConfiguration Configuration;

		public App()
		{
			this.Configuration = this.LoadConfiguration();
			this.Client = new DiscordClient(
				new DiscordConfiguration
				{
					TokenType = TokenType.Bot,
					Token = Configuration.GetValue<string>("Environment:PROD:TOKEN"),

					UseInternalLogHandler = true
				}
			);
		}

		public async Task MainAsync(string[] argv)
		{
			using (ServiceProvider services = this.ConfigureServices())
			{
				this.RegisterEvents(services);

				await this.Client.ConnectAsync();

				await Task.Delay(-1);
			}
		}

		private async Task InitializeServices(IServiceProvider services)
		{
			services.GetRequiredService<DatabaseService>().Initialize();

			services.GetRequiredService<CommandService>().InstallCommandsAsync();

			await services.GetRequiredService<MusicService>().Initialize(this.Client.UseLavalink());
		}

		private void RegisterEvents(IServiceProvider services)
		{
			this.Client.Ready += async (ReadyEventArgs args) =>
			{
				await this.InitializeServices(services);
				await services.GetRequiredService<LoggingService>().LogAsync("Ready!");
			};

			this.Client.ClientErrored += (ClientErrorEventArgs args) =>
				services.GetRequiredService<LoggingService>().LogAsync(args.Exception.ToString());
		}

		private ServiceProvider ConfigureServices() =>
			new ServiceCollection()
				.AddSingleton(Client)
				.AddSingleton(Configuration)
				.AddSingleton<Random>()
				.AddSingleton<LoggingService>()
				.AddSingleton<DatabaseService>()
				.AddSingleton<CommandService>()
				.AddSingleton<MusicService>()
				.BuildServiceProvider();

		private IConfiguration LoadConfiguration() =>
			new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json")
				.Build();
	}
}
*/