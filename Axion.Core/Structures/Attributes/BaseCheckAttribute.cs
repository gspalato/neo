using Axion.Core.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Attributes
{
	public abstract class BaseCheckAttribute : CheckAttribute
	{
		public override ValueTask<CheckResult> CheckAsync(CommandContext context)
		{
			return CheckAsync((AxionContext)context, context.ServiceProvider);
		}

		public abstract ValueTask<CheckResult> CheckAsync(AxionContext context, IServiceProvider provider);
	}
}