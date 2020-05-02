using Axion.Core.Services;
using Canducci.MongoDB.Repository.Connection;
using Discord.WebSocket;
using Qmmands;
using System;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using Discord;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

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
                .AddSingleton<Random>()
                .AddScoped<IConfig, Config>()
                .AddScoped<IConnect, Connect>()
                .AddScoped<IGuildSettingsRepository, GuildSettingsRepository>();
        }
    }
}
