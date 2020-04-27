using System;
using System.Threading.Tasks;
using Discord;
using Muon.Commands;
using Qmmands;

namespace Muon.Kernel.Structures.Attributes
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
			var context = (MuonContext)_;

			var member = await context.Guild.GetCurrentUserAsync();

			return member.GetPermissions(context.Channel).Has(Value)
				? CheckResult.Successful
				: CheckResult.Unsuccessful("You don't have enough permissions to do this.");
		}
	}
}