using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Arpa;
using Arpa.Services;

namespace Arpa.Modules
{
	public class MiscellaneousCommands : ModuleBase<SocketCommandContext>
	{
		[Command("ping")]
		[Summary("sigh.")]
		public async Task PingAsync()
		{
			await Context.Channel.SendMessageAsync("ping.");
		}
	}
}
