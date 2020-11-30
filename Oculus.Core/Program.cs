using Canducci.MongoDB.Repository.Connection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qmmands;
using Oculus.Core;
using Oculus.Core.Services;
using Oculus.Database.Services;
using Oculus.Database.Repositories;
using System;
using Victoria;
using Discord.Addons.Interactive;
using Discord.Webhook;

Host.CreateDefaultBuilder(args)
	.ConfigureServices((hostContext, services) =>
	{
		if (hostContext.Configuration is null)
		{
			services
				.BuildServiceProvider()
				.GetRequiredService<ILoggingService>()
				.Critical("No appsettings.json file was found.");
			return;
		}

		string lavalinkPassword = "";
		try
		{
			lavalinkPassword = hostContext.Configuration.GetValue<string>("LAVALINK");
		}
		catch
		{
			services
				.BuildServiceProvider()
				.GetRequiredService<ILoggingService>()
				.Critical("No Lavalink password was provided.");
			return;
		}

		services
			.AddSingleton(new DiscordSocketConfig
			{
				ExclusiveBulkDelete = true,
				
			})
			.AddSingleton(new CommandServiceConfiguration
			{
				DefaultRunMode = RunMode.Parallel
			})
			.AddSingleton(new LavaConfig
			{
				Authorization = lavalinkPassword,
				LogSeverity = LogSeverity.Debug
			});
	})
	.ConfigureServices((hostContext, services) =>
	{
		services
			.AddSingleton<DiscordSocketClient>()
			.AddSingleton<ICacheManagerService, CacheManagerService>()
			.AddSingleton<ICommandHandlingService, CommandHandlingService>()
			.AddSingleton<ICommandService, CommandService>()
			.AddSingleton<IDocumentationService, DocumentationService>()
			.AddSingleton<IEventService, EventService>()
			.AddSingleton<ILoggingService, LoggingService>()
			.AddSingleton<IMusicService, MusicService>()
			.AddSingleton<InteractiveService>()
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
			.AddScoped<ITagsRepository, TagsRepository>()
			.AddScoped<ITrustedUserRepository, TrustedUserRepository>();
	})
	.Build()
	.Run();