using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Muon;

namespace Muon.Commands
{
	public partial class Miscellaneous : BaseCommandModule
	{
		[Command("echo")]
		public async Task EchoAsync(CommandContext ctx, params string[] text) =>
			await ctx.RespondAsync(string.Join(" ", text)).ConfigureAwait(false);
	}
}