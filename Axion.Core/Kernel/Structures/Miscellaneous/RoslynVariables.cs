using Axion.Commands;
using Discord;
using Discord.WebSocket;

namespace Axion.Kernel.Structures.Miscellaneous
{
	public class RoslynVariables
	{
		public DiscordSocketClient Client;
		public IUserMessage Message;
		public AxionContext Context;
	}
}