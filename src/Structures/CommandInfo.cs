using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Arpa.Structures
{
	public interface _ICommandInfo
	{

	}

	public class _CommandInfo : _ICommandInfo
	{
		public string Id;
		public List<string> Alias = null;
		public string Group = null;
	}
}