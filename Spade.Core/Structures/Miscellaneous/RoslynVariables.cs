using Spade.Core.Commands;
using Discord;
using Discord.WebSocket;

namespace Spade.Core.Structures.Miscellaneous
{
	public record RoslynVariables
	{
		public DiscordSocketClient Client { get; init; }
		public IUserMessage Message { get; init; }
		public SpadeContext Context { get; init; }
	}
}