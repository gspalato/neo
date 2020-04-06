using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;

using Muon.Kernel;
using Muon.Services;

namespace Muon
{
	internal sealed class Program
	{
		static void Main(string[] args) =>
			CreateHostBuilder(args).Build().Run();

		static IHostBuilder CreateHostBuilder(string[] args)
			=> Host.CreateDefaultBuilder(args)
			.ConfigureLogging((hostContext, configLogging) =>
			{
				configLogging.AddConsole();
				configLogging.AddDebug();
			})
			.ConfigureServices((hostContext, services) =>
			{
				services
				.AddScoped(services =>
					new DiscordConfiguration
					{
						TokenType = TokenType.Bot,
						Token = hostContext.Configuration.GetSection("TOKEN").Value,
						UseInternalLogHandler = true
					})
				.AddSingleton<DiscordClient>()
				.AddSingleton<Random>()
				.AddSingleton<LoggingService>()
				.AddSingleton<DatabaseService>()
				.AddSingleton<CommandService>()
				.AddSingleton<MusicService>()
				.AddHostedService<App>();

				services.AddLogging();
			});
	}
}
