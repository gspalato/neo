using System;
using System.Threading.Tasks;

using Arpa.Structures;

namespace Arpa.Services
{
	public interface ICommandHandlerService
	{
		void InstallCommandsAsync(string prefix);
	}
}