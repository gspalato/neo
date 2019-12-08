using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord.WebSocket;

using Arpa.Structures;

namespace Arpa.Structures
{
	public class SocketUserParser : TypeParser<SocketUser>
	{
		public override async Task<TypeParserResult> ParseAsync(string arg, _CommandContext ctx, int position)
		{
			if (!(ulong.TryParse(arg, out ulong id)))
				return await GenerateTask(null);

			return await GenerateTask(await ctx.Client.GetUserAsync(id));
		}
	}
}