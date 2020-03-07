using System;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Muon.Core.Structures
{
	public class RoslynVariables
	{
		public DiscordClient Client;
		public DiscordMessage Message;

		public CommandContext Context;
	}
}