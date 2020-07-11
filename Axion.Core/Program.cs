using Axion.Core.Services;
using Axion.Database.Repositories;
using Canducci.MongoDB.Repository.Connection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qmmands;
using System;
using Victoria;

namespace Axion.Core
{
	internal static class Program
	{
		private static void Main(string[] args) =>
			CreateHostBuilder(args).Build().Run();

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddSingleton(new DiscordSocketConfig
						{
							ExclusiveBulkDelete = true
						})
						.AddSingleton(new CommandServiceConfiguration
						{
							DefaultRunMode = RunMode.Parallel
						})
						.AddSingleton(new LavaConfig
						{
							Authorization = hostContext.Configuration.GetValue<string>("LAVALINK"),
							LogSeverity = LogSeverity.Debug
						});
				})
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddSingleton<DiscordSocketClient>()
						.AddSingleton<ICommandHandlingService, CommandHandlingService>()
						.AddSingleton<ICommandService, CommandService>()
						.AddSingleton<IDocumentationService, DocumentationService>()
						.AddSingleton<IEventService, EventService>()
						.AddSingleton<ILoggingService, LoggingService>()
						.AddSingleton<IMusicService, MusicService>()
						.AddSingleton<LavaNode>()
						.AddSingleton<Random>()
						.AddHostedService<App>();
				})
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddScoped<IConfig, Config>()
						.AddScoped<IConnect, Connect>()
						.AddScoped<IGuildSettingsRepository, GuildSettingsRepository>()
						.AddScoped<IQueueRepository, QueueRepository>()
						.AddScoped<ITagsRepository, TagsRepository>();
				});
	}
}