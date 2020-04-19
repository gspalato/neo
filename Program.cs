using System;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muon.Kernel;
using Muon.Services;
using Qmmands;
using Victoria;

namespace Muon
{
	internal static class Program
	{
		private static void Main(string[] args) =>
			CreateHostBuilder(args).Build().Run();

		private static IHostBuilder CreateHostBuilder(string[] args)
			=> Host.CreateDefaultBuilder(args)
			.ConfigureLogging((hostContext, configLogging) =>
				configLogging
				.AddConsole()
				.AddDebug()
			)
			.ConfigureServices((hostContext, services) =>
				services
				.AddSingleton(new CommandServiceConfiguration
				{
					DefaultRunMode = RunMode.Parallel,

				})
				.AddSingleton(new LavaConfig
				{
					Authorization = hostContext.Configuration.GetValue<string>("LAVALINK"),
					LogSeverity = LogSeverity.Debug
				})
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandHandlingService>()
				.AddSingleton<ICommandService, CommandService>()
				.AddSingleton<IDatabaseService, DatabaseService>()
				.AddSingleton<IEventService, EventService>()
				.AddSingleton<IMusicService, MusicService>()
				.AddSingleton<ILoggingService, LoggingService>()
				.AddSingleton<LavaNode>()
				.AddSingleton<Random>()
				.AddHostedService<App>()
				.AddLogging()
			);
	}
}
