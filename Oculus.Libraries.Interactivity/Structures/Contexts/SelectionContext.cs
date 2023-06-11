using Discord;

namespace Oculus.Libraries.Interactivity.Structures.Contexts
{
    public class SelectionContext : InteractivityContext
    {
        public List<InteractivityComponent<ButtonBuilder, SelectionContext>> Buttons { get; internal set; } = new();
        public List<ulong> UserIds { get; internal set; } = new();

        public SelectionContext(IList<ulong> userIds,
            List<InteractivityComponent<ButtonBuilder, SelectionContext>> buttons)
            : base()
        {
            UserIds = userIds.ToList();
            Buttons = buttons.Select(x =>
            {
                var button = x.Component;
                button.WithCustomId(Guid.NewGuid().ToString());
                return new InteractivityComponent<ButtonBuilder, SelectionContext>(button, x.Callback);
            }).ToList();
        }
    }
}