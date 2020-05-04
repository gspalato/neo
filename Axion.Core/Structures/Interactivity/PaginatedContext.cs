using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
    public class PaginatedContext
    {
        public PaginatedMessage PaginatedMessage { get; set; }
        public IUser User { get; set; }
        public SocketReaction Reaction { get; set; }
	}
}
