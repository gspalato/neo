
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muon.Kernel;
using Muon.Services;
using Qmmands;
using System;
using Victoria;

namespace Muon
{
	internal sealed class Program
	{
		static void Main(string[] args) =>
			CreateHostBuilder(args).Build().Run();

		static IHostBuilder CreateHostBuilder(string[] args)
			=> Host.CreateDefaultBuilder(args)
			.ConfigureLogging((hostContext, configLogging) =>
			{
				configLogging.AddConsole();
				configLogging.AddDebug();
			})
			.ConfigureServices((hostContext, services) =>
			{
				services
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton(services =>
					new CommandServiceConfiguration
					{
						DefaultRunMode = RunMode.Parallel
					})
				.AddSingleton(services =>
					new LavaConfig
					{
						Authorization = hostContext.Configuration.GetValue<string>("LAVALINK")
					})
				.AddSingleton<LavaNode>()
				.AddSingleton<CommandHandlingService>()
				.AddSingleton<ICommandService, CommandService>()
				.AddSingleton<IDatabaseService, DatabaseService>()
				.AddSingleton<IEventService, EventService>()
				.AddSingleton<IMusicService, MusicService>()
				.AddSingleton<Random>()
				.AddHostedService<App>();

				services.AddLogging();
			});
	}
}
