using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using Discord.Rest;
using Discord.Commands;
using Oculus.Libraries.Interactivity.Structures;

namespace Oculus.Libraries.Interactivity
{
    public class ButtonRowBuilder
    {
        public List<Tuple<ButtonBuilder, Func<SocketMessageComponent, ButtonRowContext, bool>>> Buttons { get; private set; } = new();
        public List<ulong> UserIds { get; private set; } = new();

        public ButtonRowBuilder() { }

        public ButtonRowBuilder WithUser(IUser user)
        {
            if (UserIds.Contains(user.Id))
                return this;

            UserIds.Add(user.Id);
            return this;
        }

        public ButtonRowBuilder WithUsers(IEnumerable<IUser> users)
        {
            foreach (var user in users)
                WithUser(user);

            return this;
        }

        public ButtonRowBuilder WithButton(ButtonBuilder button, Func<SocketMessageComponent, ButtonRowContext, bool> handler)
        {
            Buttons.Add(new Tuple<ButtonBuilder, Func<SocketMessageComponent, ButtonRowContext, bool>>(button, handler));
            return this;
        }

        public ButtonRowContext Build()
        {
            return new ButtonRowContext(UserIds, Buttons);
        }
    }
}