﻿using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neo.Common.Configurations;
using Neo.Common.Data;
using Neo.Core;
using Neo.Core.Repositories;
using Neo.Core.Services;
using Neo.Libraries.Interactivity;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config
            .AddEnvironmentVariables(prefix: "NEO__");
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Configurations
        var config = new BaseConfiguration();
        hostContext.Configuration.Bind(config);

        services.AddSingleton(config);

        // Database Repositories
        var databaseContext = new DatabaseContext(config);
        services.AddSingleton<IDatabaseContext, DatabaseContext>(_ => databaseContext);

        services
            .AddScoped<IGuildSettingsRepository, GuildSettingsRepository>();

        var lavalinkSection = hostContext.Configuration.GetSection("Lavalink");
        services
            .AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 10
            })
            .AddSingleton(new InteractionServiceConfig
            {
                AutoServiceScopes = true,
                DefaultRunMode = RunMode.Async,
            })
            .AddSingleton(new LavalinkNodeOptions
            {
                RestUri = lavalinkSection.GetValue<string>("RestUri")!,
                WebSocketUri = lavalinkSection.GetValue<string>("WebSocketUri")!,
                Password = lavalinkSection.GetValue<string>("Password")!,
                DisconnectOnStop = false
            });

        services
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<DiscordRestClient>()
            .AddSingleton((services) =>
            {
                return new InteractionService(
                    services.GetRequiredService<DiscordSocketClient>(),
                    services.GetRequiredService<InteractionServiceConfig>()
                );
            });

        services
            .AddSingleton<CommandHandlerService>()
            .AddSingleton<InteractivityService>()
            .AddSingleton<ILogger, LoggingService>()
            .AddSingleton<ILoggingService, LoggingService>()
            .AddSingleton<SnipeService>();

        services
            .AddSingleton<IMusicService, MusicService>()
            .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();

        services
            .AddHostedService<App>();
    })
    .Build()
    .Run();