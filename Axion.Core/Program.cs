using Axion.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Axion.Core
{
	internal static class Program
	{
		private static void Main(string[] args) =>
			CreateHostBuilder(args).Build().Run();

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration((hostContext, configBuilder) =>
			{
				configBuilder.AddJsonFile("appsettings.json");
			})
			.ConfigureLogging((hostContext, configLogging) =>
			{
				configLogging
					.AddConsole()
					.AddDebug();
			})
			.ConfigureServices((hostContext, services) =>
			{
				services
					.AddAxionCoreServices(hostContext)
                    .AddAxionDatabases()
					.AddHostedService<App>()
					.AddLogging();
			});
	}
}