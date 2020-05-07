using Axion.Core.Commands;
using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequireChannelUserPermissionsAttribute : CheckAttribute
	{
		private ChannelPermission Value { get; }

		public RequireChannelUserPermissionsAttribute(ChannelPermission permissions)
		{
			Value = permissions;
		}

		public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
		{
			var context = (AxionContext)_;

			var app = await context.Client.GetApplicationInfoAsync();
			if (app.Owner.Id == context.User.Id)
				return CheckResult.Successful;

			var member = await context.Guild.GetUserAsync(context.User.Id);

			return member.GetPermissions(context.Channel).Has(Value)
				? CheckResult.Successful
				: CheckResult.Unsuccessful("You don't have enough permissions to do this.");
		}
	}
}