using Axion.Core.Commands;
using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequireGuildUserPermissionsAttribute : CheckAttribute
	{
		private GuildPermission Value { get; }

		public RequireGuildUserPermissionsAttribute(GuildPermission permissions)
		{
			Value = permissions;
		}

		public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
		{
			var context = (AxionContext)_;

			IApplication app = await context.Client.GetApplicationInfoAsync();
			if (app.Owner.Id == context.User.Id)
				return CheckResult.Successful;

			var member = await context.Guild.GetUserAsync(context.User.Id);

			return member.GuildPermissions.Has(Value)
				? CheckResult.Successful
				: CheckResult.Unsuccessful("You don't have enough permissions to do this.");
		}
	}
}