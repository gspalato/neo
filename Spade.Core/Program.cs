using Canducci.MongoDB.Repository.Connection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qmmands;
using Spade.Core;
using Spade.Core.Services;
using Spade.Database.Services;
using Spade.Database.Repositories;
using System;
using Victoria;


Host.CreateDefaultBuilder(args)
	.ConfigureServices((hostContext, services) =>
	{
		string lavalinkPassword = hostContext.Configuration.GetValue<string>("LAVALINK");
		if (lavalinkPassword.Length == 0)
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
				ExclusiveBulkDelete = true
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