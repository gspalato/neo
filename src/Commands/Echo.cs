using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Arpa;


namespace Arpa.Commands
{
	public partial class Miscellaneous : BaseCommandModule
	{
		[Command("echo")]
		public async Task EchoAsync(CommandContext ctx, params string[] text) =>
			await ctx.RespondAsync(string.Join(" ", text)).ConfigureAwait(false);
	}
}