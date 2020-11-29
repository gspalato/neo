using Qmmands;
using Oculus.Core.Commands;
using Oculus.Database.Repositories;
using System;
using System.Threading.Tasks;

namespace Oculus.Core.Structures.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequireTrustedUserAttribute : BaseCheckAttribute
	{
		public override async ValueTask<CheckResult> CheckAsync(OculusContext context, IServiceProvider provider)
		{
			var ownerCheck = await new RequireOwnerAttribute().CheckAsync(context, provider);
			if (ownerCheck.IsSuccessful)
				return CheckResult.Successful;

			ITrustedUserRepository trustedUserRepository = context.GetService<ITrustedUserRepository>();

			bool isTrusted = await trustedUserRepository.IsTrusted(context.User.Id);

			if (isTrusted)
				return CheckResult.Successful;

			return CheckResult.Unsuccessful("You lack permissions to execute this command.");
		}
	}
}
