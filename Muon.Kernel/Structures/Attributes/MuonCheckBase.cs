using System;
using System.Threading.Tasks;

using Qmmands;

using Muon.Commands;

namespace Muon.Kernel.Structures.Attributes
{
	public abstract class MuonCheckBase : CheckAttribute
	{
		public override ValueTask<CheckResult> CheckAsync(CommandContext context)
		{
			return CheckAsync((MuonContext)context, context.ServiceProvider);
		}

		public abstract ValueTask<CheckResult> CheckAsync(MuonContext context, IServiceProvider provider);
	}
}
