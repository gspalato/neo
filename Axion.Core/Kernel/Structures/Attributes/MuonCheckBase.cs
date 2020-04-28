using Axion.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Kernel.Structures.Attributes
{
	public abstract class AxionCheckBase : CheckAttribute
	{
		public override ValueTask<CheckResult> CheckAsync(CommandContext context)
		{
			return CheckAsync((AxionContext)context, context.ServiceProvider);
		}

		public abstract ValueTask<CheckResult> CheckAsync(AxionContext context, IServiceProvider provider);
	}
}
