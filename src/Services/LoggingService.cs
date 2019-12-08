using System;
using System.Threading.Tasks;

using Discord;

namespace Arpa.Services
{
	public interface ILoggingService
	{
		Task LogAsync(LogMessage log);
	}

	public class LoggingService : ILoggingService
	{
		public LoggingService()
		{

		}

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