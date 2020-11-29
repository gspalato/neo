using Oculus.Core.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Oculus.Core.Structures.Attributes
{
	public abstract class BaseCheckAttribute : CheckAttribute
	{
		public override ValueTask<CheckResult> CheckAsync(CommandContext context)
		{
			return CheckAsync((OculusContext)context, context.ServiceProvider);
		}

		public abstract ValueTask<CheckResult> CheckAsync(OculusContext context, IServiceProvider provider);
	}
}