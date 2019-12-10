using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord.WebSocket;

using Arpa.Entities;
using Arpa.Errors;

namespace Arpa.Structures
{
	public class SocketUserParser : ITypeParser<SocketUser>
	{
		public async Task<SocketUser> ParseAsync(string arg, CommandContext ctx, int position)
		{
			if (!(ulong.TryParse(arg, out ulong id)))
				throw new ArgumentParsingException("SocketUser");

			return (await Task.FromResult(await ctx.Client.GetUserAsync(id))) as SocketUser;
		}
	}
}