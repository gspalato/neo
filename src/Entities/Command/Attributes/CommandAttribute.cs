using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace Arpa.Entities
{
	[AttributeUsage(AttributeTargets.Class)]
	public class _CommandAttribute : Attribute
	{
		public readonly string Id;

		public _CommandAttribute(string id)
		{
			this.Id = id;
		}
	}
}