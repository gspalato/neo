using Discord.WebSocket;

namespace Oculus.Libraries.Interactivity
{
    public abstract class InteractivityContext
    {
        public int Timeout { get; private set; } = 60;
        public TaskCompletionSource TimeoutCompletionSource { get; private set; } = new TaskCompletionSource();
        public Func<SocketMessageComponent, InteractivityContext, bool> TimeoutCallback { get; private set; }

        public InteractivityContext(int timeout = 60, Func<SocketMessageComponent, InteractivityContext, bool> callback = default)
        {
            Timeout = timeout;
            TimeoutCallback = callback;
        }
    }
}