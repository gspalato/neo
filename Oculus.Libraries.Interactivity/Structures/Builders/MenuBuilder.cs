using Discord;
using Discord.WebSocket;
using Oculus.Libraries.Interactivity.Structures.Contexts;

namespace Oculus.Libraries.Interactivity.Structures.Builders
{
    public class MenuBuilder
    {
        public SelectMenuBuilder Menu { get; private set; } = new();
        public List<InteractivityComponent<SelectMenuOptionBuilder, MenuContext>> Options { get; private set; } = new();
        public List<ulong> UserIds { get; private set; } = new();

        public MenuBuilder WithUser(IUser user)
        {
            if (UserIds.Contains(user.Id))
                return this;

            UserIds.Add(user.Id);
            return this;
        }

        public MenuBuilder WithUsers(IEnumerable<IUser> users)
        {
            foreach (var user in users)
                WithUser(user);

            return this;
        }

        public MenuBuilder WithMenu(SelectMenuBuilder menu)
        {
            menu.Options.RemoveAll((o) => true);
            Menu = menu;

            return this;
        }

        public MenuBuilder WithOption(SelectMenuOptionBuilder button, Func<SocketMessageComponent, MenuContext, string, bool> handler)
        {
            Options.Add(new InteractivityComponent<SelectMenuOptionBuilder, MenuContext>(button, handler));
            return this;
        }

        internal MenuContext Build(string interactivityId)
        {
            return new MenuContext(interactivityId, UserIds, false, Menu, Options);
        }
    }
}