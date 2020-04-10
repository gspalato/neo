using Discord;

namespace Muon.Commands.Results
{
	public class ResultCompletionData
	{
		public IUserMessage Message { get; }

		public ResultCompletionData(IUserMessage message)
			=> Message = message;
	}
}