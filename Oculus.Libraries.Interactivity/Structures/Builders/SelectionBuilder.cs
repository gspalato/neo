using Discord;
using Discord.WebSocket;
using Oculus.Libraries.Interactivity.Structures.Contexts;

namespace Oculus.Libraries.Interactivity.Structures.Builders
{
    public class SelectionBuilder
    {
        public List<InteractivityComponent<ButtonBuilder, SelectionContext>> Buttons { get; private set; } = new();
        public List<ulong> UserIds { get; private set; } = new();

        public SelectionBuilder() { }

        public SelectionBuilder WithUser(IUser user)
        {
            if (UserIds.Contains(user.Id))
                return this;

            UserIds.Add(user.Id);
            return this;
        }

        public SelectionBuilder WithUsers(IEnumerable<IUser> users)
        {
            foreach (var user in users)
                WithUser(user);

            return this;
        }

        public SelectionBuilder WithButton(ButtonBuilder button, Func<SocketMessageComponent, SelectionContext, bool> handler)
        {
            Buttons.Add(new InteractivityComponent<ButtonBuilder, SelectionContext>(button, handler));
            return this;
        }

        public SelectionContext Build()
        {
            return new SelectionContext(UserIds, Buttons);
        }
    }
}