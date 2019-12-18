using System;
using System.Threading.Tasks;

using Qmmands;

namespace Arpa.Entities.Results
{
	public abstract class ArpaCommandResult : CommandResult
	{
		public override bool IsSuccessful { get; }

		public ArpaCommandResult(bool success) =>
			this.IsSuccessful = success;
	}
}