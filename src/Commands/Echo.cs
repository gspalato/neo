using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Arpa;


namespace Arpa.Commands
{
	[Description("Special snowflakes that don't fit on other groups.")]
	public partial class Miscellaneous : BaseCommandModule
	{
		[Command("echo")]
		public async Task EchoAsync(CommandContext ctx, params string[] text) =>
			await ctx.RespondAsync(string.Join(" ", text));
	}
}