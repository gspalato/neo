using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Muon.Commands
{
	public partial class Moderation : BaseCommandModule
	{
		[Command("sudo")]
		[Description("Executes a command as another user.")]
		[Hidden]
		[RequireOwner]
		public Task Sudo(CommandContext ctx,
			[Description("Member to execute as.")] DiscordMember member,
			[RemainingText, Description("Command text to execute.")] string command)
		{
			return Task.CompletedTask;
		}
	}
}