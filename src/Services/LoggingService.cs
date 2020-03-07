using System;
using System.Threading.Tasks;

using DSharpPlus;

namespace Muon.Services
{
	public interface ILoggingService
	{
		Task LogAsync(string log);
	}

	public class LoggingService : ILoggingService
	{
		public Task LogAsync(string log)
		{
			Console.WriteLine(log);
			return Task.CompletedTask;
		}
	}
}