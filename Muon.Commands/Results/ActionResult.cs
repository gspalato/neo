using Qmmands;
using System.Threading.Tasks;

namespace Muon.Commands.Results
{
	public abstract class ActionResult : CommandResult
	{
		public override bool IsSuccessful { get; } = true;

		public abstract ValueTask<ResultCompletionData> ExecuteResultAsync(MuonContext ctx);

		public static implicit operator Task<ActionResult>(ActionResult res)
			=> Task.FromResult(res);
	}
}