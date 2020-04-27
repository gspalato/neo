using Discord;
using Discord.WebSocket;
using Muon.Commands;

namespace Muon.Kernel.Structures.Miscellaneous
{
	public class RoslynVariables
	{
		public DiscordSocketClient Client;
		public IUserMessage Message;
		public MuonContext Context;
	}
}