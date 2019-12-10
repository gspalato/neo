using System;
using System.Threading.Tasks;

using Discord;

using Arpa.Structures;

namespace Arpa.Services
{
	public interface ILoggingService
	{
		Task LogAsync(LogMessage log);
	}
}