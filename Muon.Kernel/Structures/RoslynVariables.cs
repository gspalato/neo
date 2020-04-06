using System;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Muon.Kernel.Structures
{
	public class RoslynVariables
	{
		public DiscordClient Client;
		public DiscordMessage Message;

		public CommandContext Context;
	}
}