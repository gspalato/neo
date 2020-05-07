using Axion.Core.Commands;
using Discord;
using Discord.WebSocket;

namespace Axion.Core.Structures.Miscellaneous
{
	public class RoslynVariables
	{
		public DiscordSocketClient Client;
		public IUserMessage Message;
		public AxionContext Context;
	}
}