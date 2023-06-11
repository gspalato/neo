using Discord;
using Discord.WebSocket;

namespace Oculus.Libraries.Interactivity
{
    public class InteractivityComponent<TComponent, TContext> where TContext : InteractivityContext
    {
        public TComponent Component { get; private set; }
        public Func<SocketMessageComponent, TContext, bool> Callback { get; private set; }

        public InteractivityComponent(TComponent component, Func<SocketMessageComponent, TContext, bool> callback)
        {
            Component = component;
            Callback = callback;
        }
    }
}