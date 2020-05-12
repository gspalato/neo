using Axion.Core.Database;
using Axion.Core.Services;
using Canducci.MongoDB.Repository.Connection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qmmands;
using System;
using System.Linq;
using Victoria;

namespace Axion.Core.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IHostBuilder AddAxionCoreConfigurations(this IHostBuilder builder, string[] args)
        {
            return builder.ConfigureServices((hostContext, services) =>
            {
                var config = new DiscordSocketConfig
                {
                    ExclusiveBulkDelete = true
                };

                if (args.ElementAtOrDefault(0) != null)
                {
                    config.ShardId = int.Parse(args[0]);
                }

                services
                    .AddSingleton(config)
                    .AddSingleton(new CommandServiceConfiguration
                    {
                        DefaultRunMode = RunMode.Parallel
                    })
                    .AddSingleton(new LavaConfig
                    {
                        Authorization = hostContext.Configuration.GetValue<string>("LAVALINK"),
                        LogSeverity = LogSeverity.Debug
                    });
            });
        }

        public static IHostBuilder AddAxionCoreServices(this IHostBuilder builder)
        {
            return builder.ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton<DiscordSocketClient>()
                    .AddSingleton<CommandHandlingService>()
                    .AddSingleton<ICommandService, CommandService>()
                    .AddSingleton<IDocumentationService, DocumentationService>()
                    .AddSingleton<IEventService, EventService>()
                    .AddSingleton<ILoggingService, LoggingService>()
                    .AddSingleton<IMusicService, MusicService>()
                    .AddSingleton<LavaNode>()
                    .AddSingleton<Random>()
                    .AddHostedService<App>()
                    .AddLogging();
            });
        }

        public static IHostBuilder AddAxionDatabases(this IHostBuilder builder)
        {
            return builder.ConfigureServices((hostContext, services) =>
            {
                services
                    .AddScoped<IConfig, Config>()
                    .AddScoped<IConnect, Connect>()
                    .AddScoped<IGuildSettingsRepository, GuildSettingsRepository>()
                    .AddScoped<ITagsRepository, TagsRepository>();
            });
        }
    }
}