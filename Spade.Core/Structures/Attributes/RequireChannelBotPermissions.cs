using Spade.Core.Commands;
using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Spade.Core.Structures.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequireChannelBotPermissionsAttribute : CheckAttribute
	{
		private ChannelPermission Value { get; }

		public RequireChannelBotPermissionsAttribute(ChannelPermission permissions)
		{
			Value = permissions;
		}

		public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
		{
			var context = (SpadeContext)_;

			if (context.Channel is not ITextChannel textChannel)
				return CheckResult.Unsuccessful("This command's not available on DMs.");

			var member = await context.Guild.GetCurrentUserAsync();

			return member.GetPermissions(textChannel).Has(Value)
				? CheckResult.Successful
				: CheckResult.Unsuccessful("You don't have enough permissions to do this.");
		}
	}
}