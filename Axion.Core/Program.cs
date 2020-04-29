using Axion.Kernel;
using Axion.Services;
using Canducci.MongoDB.Repository.Connection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using Victoria;

namespace Axion
{
	internal static class Program
	{
		private static void Main(string[] args) =>
			CreateHostBuilder(args).Build().Run();

		private static IHostBuilder CreateHostBuilder(string[] args)
			=> Host.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration((hostContext, configBuilder) =>
				configBuilder.AddJsonFile("appsettings.json"))
			.ConfigureLogging((hostContext, configLogging) =>
				configLogging
				.AddConsole()
				.AddDebug()
			)
			.ConfigureServices((hostContext, services) =>
				services
				.AddSingleton(new CommandServiceConfiguration
				{
					DefaultRunMode = RunMode.Parallel
				})
				.AddSingleton(new LavaConfig
				{
					Authorization = hostContext.Configuration.GetValue<string>("LAVALINK"),
					LogSeverity = LogSeverity.Debug
				})
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandHandlingService>()
				.AddSingleton<ICommandService, CommandService>()
				.AddSingleton<IEventService, EventService>()
				.AddSingleton<IMusicService, MusicService>()
				.AddSingleton<ILoggingService, LoggingService>()
				.AddSingleton<LavaNode>()
				.AddSingleton<Random>()
				.AddScoped<IConfig, Config>()
				.AddScoped<IConnect, Connect>()
				.AddScoped<IGuildSettingsRepository, GuildSettingsRepository>()
				.AddHostedService<App>()
				.AddLogging()
			);
	}
}
