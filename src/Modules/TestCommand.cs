using System;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Arpa;
using Arpa.Entities;
using Arpa.Services;
using Arpa.Structures;

namespace Arpa.Modules
{
	[_CommandAttribute("say")]
	public class TestCommand : _Command
	{
		public Task RunAsync(SocketUser user)
		{
			Console.WriteLine(user);

			return Task.CompletedTask;
		}
	}
}
