using Spade.Core.Commands;
using Discord;
using Discord.WebSocket;

namespace Spade.Core.Structures
{
	public class RoslynVariables
	{
		public DiscordSocketClient Client;
		public IUserMessage Message;
		public SpadeContext Context;
	}
}