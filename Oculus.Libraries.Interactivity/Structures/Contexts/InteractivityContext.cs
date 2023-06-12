using Discord.WebSocket;

namespace Oculus.Libraries.Interactivity
{
    public abstract class InteractivityContext
    {
        public string InteractivitySessionId { get; internal set; } = string.Empty;

        public int Timeout { get; private set; } = 60;
        public TaskCompletionSource TimeoutCompletionSource { get; private set; } = new TaskCompletionSource();
        public Func<SocketMessageComponent, InteractivityContext, bool> TimeoutCallback { get; private set; }

        internal InteractivityContext(string sessionId, int timeout = 60, Func<SocketMessageComponent, InteractivityContext, bool>? callback = null)
        {
            InteractivitySessionId = sessionId;
            Timeout = timeout;
            TimeoutCallback = callback ?? ((_, _) => false);
        }
    }
}