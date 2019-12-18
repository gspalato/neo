using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.EventArgs;

using Qmmands;

using Arpa.Services;


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
				await RegisterEvents(services);

				await this.Client.ConnectAsync();

				await Task.Delay(-1);
			}
		}

		private Task RegisterEvents(ServiceProvider services)
		{
			this.Client.Ready += async (ReadyEventArgs args) =>
				{
					services
						.GetRequiredService<CommandHandlerService>()
						.InstallCommandsAsync(Configuration["Environment:PROD:PREFIX"]);

					await services.GetRequiredService<LoggingService>().LogAsync("Ready!");
				};

			this.Client.ClientErrored += (ClientErrorEventArgs args) =>
				services.GetRequiredService<LoggingService>().LogAsync(args.Exception.ToString());

			return Task.CompletedTask;
		}

		private ServiceProvider ConfigureServices()
		{
			CommandService commandService = new CommandService(
				new CommandServiceConfiguration { DefaultRunMode = RunMode.Parallel }
			);

			return new ServiceCollection()
				.AddSingleton(Client)
				.AddSingleton(Configuration)
				.AddSingleton(commandService)
				.AddSingleton<Random>()
				.AddSingleton<LoggingService>()
				.AddSingleton<DatabaseService>()
				.AddSingleton<CommandHandlerService>()
				.BuildServiceProvider();
		}

		private IConfiguration LoadConfiguration()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();
		}
	}
}
