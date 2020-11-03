using Spade.Core.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Spade.Core.Structures.Attributes
{
	public abstract class BaseCheckAttribute : CheckAttribute
	{
		public override ValueTask<CheckResult> CheckAsync(CommandContext context)
		{
			return CheckAsync((SpadeContext)context, context.ServiceProvider);
		}

		public abstract ValueTask<CheckResult> CheckAsync(SpadeContext context, IServiceProvider provider);
	}
}