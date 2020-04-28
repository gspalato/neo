using Axion.Commands;
using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Kernel.Structures.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequireBotPermissionsAttribute : CheckAttribute
	{
		private ChannelPermission Value { get; }

		public RequireBotPermissionsAttribute(ChannelPermission permissions)
		{
			Value = permissions;
		}

		public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
		{
			var context = (AxionContext)_;
			var current = await context.Guild.GetCurrentUserAsync();

			return current.GetPermissions(context.Channel).Has(Value)
				? CheckResult.Successful
				: CheckResult.Unsuccessful("I don't have enough permissions to do this.");
		}
	}
}
