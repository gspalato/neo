using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;

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
		public DiscordSocketClient Client = new DiscordSocketClient();
		public IConfiguration Configuration;

		public App()
		{
			Configuration = this.LoadConfiguration();
		}

		public async Task MainAsync(string[] argv)
		{
			using (ServiceProvider services = this.ConfigureServices())
			{

				this.Client.Log += services.GetRequiredService<LoggingService>().LogAsync;

				this.Client.Ready += async () =>
				{
					await services
						.GetRequiredService<CommandHandlerService>()
						.InstallCommandsAsync(Configuration["Environment:DEV:PREFIX"]);
				};

				await this.Client.LoginAsync(TokenType.Bot, Configuration["Environment:DEV:TOKEN"]);
				await this.Client.StartAsync();

				await Task.Delay(-1);
			}
		}

		private ServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddSingleton(this.Client)
				.AddSingleton(Configuration)
				.AddSingleton<LoggingService>()
				.AddSingleton<DatabaseService>()
				.AddSingleton<CommandService>()
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
