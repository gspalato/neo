using System;
using System.Threading.Tasks;

using DSharpPlus;

using Arpa.Structures;

namespace Arpa.Services
{
	public interface ILoggingService
	{
		Task LogAsync(string log);
	}
}