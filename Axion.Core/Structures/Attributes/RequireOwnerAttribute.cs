using Axion.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Attributes
{
	public class RequireOwnerAttribute : BaseCheckAttribute
	{
		public override async ValueTask<CheckResult> CheckAsync(AxionContext context, IServiceProvider provider)
		{
			var app = await context.Client.GetApplicationInfoAsync();

			if (app.Owner.Id == context.User.Id || context.Client.CurrentUser.Id == context.User.Id)
				return CheckResult.Successful;

			return CheckResult.Unsuccessful("You lack permissions to execute this command.");
		}
	}
}
