using System;
using System.Threading.Tasks;

using Discord;

namespace Arpa.Services
{
	public class LoggingService : ILoggingService
	{
		public Task LogAsync(LogMessage log)
		{
			Console.WriteLine(log.ToString());
			return Task.CompletedTask;
		}

		public Task LogAsync(string log)
		{
			Console.WriteLine(log);
			return Task.CompletedTask;
		}
	}
}