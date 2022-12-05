using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Fergun.Interactive;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oculus.Database.Services;
using Oculus.Kernel;
using Oculus.Kernel.Services;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            })
            .AddSingleton(new InteractionServiceConfig
            {
                AutoServiceScopes = true,
                DefaultRunMode = RunMode.Async,
            })
            .AddSingleton(new LavalinkNodeOptions
            {
                RestUri = hostContext.Configuration.GetValue<string>("LAVALINK:RestHost")!,
                WebSocketUri = hostContext.Configuration.GetValue<string>("LAVALINK:WebsocketHost")!,
                Password = hostContext.Configuration.GetValue<string>("LAVALINK:Password")!,
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
            .AddSingleton<DatabaseService>()
            .AddSingleton<ILogger, LoggingService>()
            .AddSingleton<ILoggingService, LoggingService>();

        services
            .AddSingleton(
                new InteractiveConfig
                {
                    DefaultTimeout = TimeSpan.FromMinutes(5),
                    ReturnAfterSendingPaginator = true
                }
            )
            .AddSingleton<InteractiveService>();

        services
            .AddSingleton<IMusicService, MusicService>()
            .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();

        services
            .AddHostedService<App>();
    })
    .Build()
    .Run();