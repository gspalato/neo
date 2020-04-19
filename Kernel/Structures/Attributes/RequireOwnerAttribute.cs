using System;
using System.Threading.Tasks;
using Muon.Commands;
using Qmmands;

namespace Muon.Kernel.Structures.Attributes
{
	public class RequireOwnerAttribute : MuonCheckBase
	{
		public override async ValueTask<CheckResult> CheckAsync(MuonContext context, IServiceProvider provider)
		{
			var app = await context.Client.GetApplicationInfoAsync();

			if (app.Owner.Id == context.User.Id || context.Client.CurrentUser.Id == context.User.Id)
				return CheckResult.Successful;

			return CheckResult.Unsuccessful("You lack permissions to execute this command.");
		}
	}
}
