using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace Arpa.Structures
{
	public interface _ICommandAttributes
	{

	}

	public class _CommandAttributes : _ICommandAttributes
	{
		public string Id;
		public List<string> Alias;
		public string Name;
		public string Group;

		public _CommandAttributes(string id, List<string> alias = null, string group = "")
		{
			this.Id = id;
			this.Alias = alias;
			this.Name = alias?.First() ?? id;
			this.Group = group;
		}
	}
}