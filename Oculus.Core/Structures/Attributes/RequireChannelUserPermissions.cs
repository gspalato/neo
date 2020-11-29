using Oculus.Core.Commands;
using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Oculus.Core.Structures.Attributes
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
			var context = (OculusContext)_;

			if (context.Channel is not ITextChannel textChannel)
				return CheckResult.Unsuccessful("This command's not available on DMs.");

			var app = await context.Client.GetApplicationInfoAsync();
			if (app.Owner.Id == context.User.Id)
				return CheckResult.Successful;

			var member = context.PermissionsUser;

			return member.GetPermissions(textChannel).Has(Value)
				? CheckResult.Successful
				: CheckResult.Unsuccessful("You don't have enough permissions to do this.");
		}
	}
}