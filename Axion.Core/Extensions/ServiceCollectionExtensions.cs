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
using Victoria;

namespace Axion.Core.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddAxionCoreConfigurations(this IServiceCollection provider, HostBuilderContext hostContext)
        {
            return provider
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
        }

        public static IServiceCollection AddAxionCoreServices(this IServiceCollection provider, HostBuilderContext hostContext)
        {
            return provider
                .AddAxionCoreConfigurations(hostContext)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<ICommandService, CommandService>()
                .AddSingleton<IDocumentationService, DocumentationService>()
                .AddSingleton<IEventService, EventService>()
                .AddSingleton<ILoggingService, LoggingService>()
                .AddSingleton<IMusicService, MusicService>()
                .AddSingleton<LavaNode>()
                .AddSingleton<Random>();
        }

        public static IServiceCollection AddAxionDatabases(this IServiceCollection provider)
        {
            return provider
                .AddScoped<IConfig, Config>()
                .AddScoped<IConnect, Connect>()
                .AddScoped<IGuildSettingsRepository, GuildSettingsRepository>();
        }
    }
}