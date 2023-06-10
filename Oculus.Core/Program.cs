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
using Oculus.Common.Configurations;
using Oculus.Common.Data;
using Oculus.Core;
using Oculus.Core.Repositories;
using Oculus.Core.Services;
using Oculus.Libraries.Interactivity;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config
            .AddEnvironmentVariables(prefix: "OCULUS__");
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
                GatewayIntents = GatewayIntents.All
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
            .AddSingleton<ILogger, LoggingService>()
            .AddSingleton<ILoggingService, LoggingService>();

        services
            .AddSingleton<InteractivityService>();

        services
            .AddSingleton<IMusicService, MusicService>()
            .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();

        services
            .AddHostedService<App>();
    })
    .Build()
    .Run();