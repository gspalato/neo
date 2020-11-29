using Oculus.Core.Commands;
using Discord;
using Discord.WebSocket;

namespace Oculus.Core.Structures
{
	public record RoslynVariables
	{
		public DiscordSocketClient Client { get; init; }
		public IUserMessage Message { get; init; }
		public OculusContext Context { get; init; }
	}
}