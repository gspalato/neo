using System;

using Discord;
using Discord.WebSocket;

using Muon.Commands;

namespace Muon.Kernel.Structures
{
	public class RoslynVariables
	{
		public DiscordSocketClient Client;
		public SocketUserMessage Message;

		public MuonContext Context;
	}
}