using Discord;
using Discord.WebSocket;

namespace Oculus.Libraries.Interactivity.Structures
{
    public class ButtonRowContext
    {
        public List<Tuple<ButtonBuilder, Func<SocketMessageComponent, ButtonRowContext, bool>>> Buttons { get; internal set; } = new();
        public List<ulong> UserIds { get; internal set; } = new();

        public ButtonRowContext(IList<ulong> userIds,
            IList<Tuple<ButtonBuilder, Func<SocketMessageComponent, ButtonRowContext, bool>>> buttons)
        {
            UserIds = userIds.ToList();
            Buttons = buttons.Select(x =>
            {
                var button = x.Item1;
                button.WithCustomId(Guid.NewGuid().ToString());
                return new Tuple<ButtonBuilder, Func<SocketMessageComponent, ButtonRowContext, bool>>(button, x.Item2);
            }).ToList();
        }
    }
}