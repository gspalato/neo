using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;

using Arpa;
using Arpa.Services;
using Arpa.Structures;


namespace Arpa
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
					Token = Configuration["Environment:PROD:TOKEN"],

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
			DatabaseService databaseService = services.GetRequiredService<DatabaseService>();
			databaseService.Initialize();

			CommandService commandService = services.GetRequiredService<CommandService>();
			commandService.InstallCommandsAsync();

			MusicService musicService = services.GetRequiredService<MusicService>();
			await musicService.Initialize(this.Client.UseLavalink());
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
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();
	}
}