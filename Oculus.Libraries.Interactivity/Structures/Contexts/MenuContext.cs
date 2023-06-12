using Discord;
using Discord.WebSocket;

namespace Oculus.Libraries.Interactivity.Structures.Contexts
{
    public class MenuContext : InteractivityContext
    {
        public List<string> SelectedOptions { get; internal set; } = new();
        public bool IsDisabled { get; private set; }
        public ComponentType ComponentType { get; private set; }

        public SelectMenuBuilder Menu { get; private set; }
        public List<InteractivityComponent<SelectMenuOptionBuilder, MenuContext>> Options { get; private set; } = new();
        public List<ulong> UserIds { get; private set; } = new();

        internal MenuContext(string sessionId, List<ulong> userIds, bool isDisabled, SelectMenuBuilder menu,
            List<InteractivityComponent<SelectMenuOptionBuilder, MenuContext>> options)
            : base(sessionId)
        {
            UserIds = userIds;
            IsDisabled = isDisabled;
            Menu = menu;
            Options = options.ToList();
        }
    }
}