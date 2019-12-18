using System;

using Qmmands;

namespace Arpa.Entities.Results
{
	public class ArpaFailedResult : ArpaCommandResult
	{
		public string Reason { get; }

		public ArpaFailedResult(string reason) : base(false) =>
			this.Reason = reason;
	}
}