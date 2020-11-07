using Spade.Core.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Spade.Core.Structures.Attributes
{
	public class RequireOwnerAttribute : BaseCheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(SpadeContext context, IServiceProvider provider)
		{
			var app = await context.Client.GetApplicationInfoAsync();

			if (app.Owner.Id == context.PermissionsUser.Id || context.Client.CurrentUser.Id == context.User.Id)
				return CheckResult.Successful;

			return CheckResult.Unsuccessful("You lack permissions to execute this command.");
		}
	}
}
