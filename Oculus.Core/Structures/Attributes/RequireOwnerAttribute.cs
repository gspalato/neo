using Oculus.Core.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Oculus.Core.Structures.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequireOwnerAttribute : BaseCheckAttribute
	{
		public override async ValueTask<CheckResult> CheckAsync(OculusContext context, IServiceProvider provider)
		{
			var app = await context.Client.GetApplicationInfoAsync();

			if (app.Owner.Id == context.PermissionsUser.Id || context.Client.CurrentUser.Id == context.User.Id)
				return CheckResult.Successful;

			return CheckResult.Unsuccessful("You lack permissions to execute this command.");
		}
	}
}
