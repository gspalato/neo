using Discord;

namespace Neo.Libraries.Interactivity.Structures.Contexts
{
    public class SelectionContext : InteractivityContext
    {
        public List<InteractivityComponent<ButtonBuilder, SelectionContext>> Buttons { get; private set; } = new();
        public List<ulong> UserIds { get; private set; } = new();

        public SelectionContext(string sessionId, List<ulong> userIds,
            List<InteractivityComponent<ButtonBuilder, SelectionContext>> buttons)
            : base(sessionId)
        {
            UserIds = userIds.ToList();
            Buttons = buttons.ToList();
        }
    }
}