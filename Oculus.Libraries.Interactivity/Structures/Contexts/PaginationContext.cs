using Discord;
using Discord.WebSocket;

namespace Oculus.Libraries.Interactivity.Structures.Contexts
{
    public class PaginationContext : InteractivityContext
    {
        public int CurrentPage { get; internal set; }

        public List<InteractivityComponent<ButtonBuilder, PaginationContext>> Buttons { get; internal set; } = new();
        public List<ulong> UserIds { get; internal set; } = new();
        public List<Embed> Pages { get; internal set; } = new();

        public PaginationContext(IList<Embed> pages, IList<ulong> userIds,
            List<InteractivityComponent<ButtonBuilder, PaginationContext>> buttons)
            : base()
        {
            Pages = pages.ToList();
            UserIds = userIds.ToList();
            Buttons = buttons.Select(x =>
            {
                var button = x.Component;
                button.WithCustomId(Guid.NewGuid().ToString());
                return new InteractivityComponent<ButtonBuilder, PaginationContext>(button, x.Callback);
            }).ToList();
        }
    }
}