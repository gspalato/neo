using Discord;
using Discord.WebSocket;

namespace Oculus.Libraries.Interactivity.Structures.Contexts
{
    public class PaginationContext : InteractivityContext
    {
        public int CurrentPage { get; internal set; }

        public List<InteractivityComponent<ButtonBuilder, PaginationContext>> Buttons { get; private set; } = new();
        public List<ulong> UserIds { get; private set; } = new();
        public List<Embed> Pages { get; private set; } = new();

        internal PaginationContext(string sessionId, List<Embed> pages, List<ulong> userIds,
            List<InteractivityComponent<ButtonBuilder, PaginationContext>> buttons)
            : base(sessionId)
        {
            Pages = pages.ToList();
            UserIds = userIds.ToList();
            Buttons = buttons.ToList();
        }
    }
}