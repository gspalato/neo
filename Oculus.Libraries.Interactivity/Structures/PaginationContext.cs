using Discord;
using Discord.WebSocket;

namespace Oculus.Libraries.Interactivity.Structures
{
    public class PaginationContext
    {
        public int CurrentPage { get; internal set; }

        public List<Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>> Buttons { get; internal set; } = new();
        public List<ulong> UserIds { get; internal set; } = new();
        public List<Embed> Pages { get; internal set; } = new();

        public PaginationContext(IList<Embed> pages, IList<ulong> userIds,
            IList<Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>> buttons)
        {
            Pages = pages.ToList();
            UserIds = userIds.ToList();
            Buttons = buttons.Select(x =>
            {
                var button = x.Item1;
                button.WithCustomId(Guid.NewGuid().ToString());
                return new Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>(button, x.Item2);
            }).ToList();
        }
    }
}