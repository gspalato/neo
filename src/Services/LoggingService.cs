using System;
using System.Threading.Tasks;

using DSharpPlus;

namespace Arpa.Services
{
	public class LoggingService : ILoggingService
	{
		public Task LogAsync(string log)
		{
			Console.WriteLine(log);
			return Task.CompletedTask;
		}
	}
}