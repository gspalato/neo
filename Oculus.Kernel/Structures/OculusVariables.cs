using Discord;

namespace Oculus.Kernel.Structures
{
    public class OculusVariables
    {
        public IDiscordInteraction Interaction { get { return Context.Interaction; } }
        public ITextChannel Channel { get { return (ITextChannel)Context.Channel; } }
        public IGuild Guild { get { return Channel.Guild; } }
        public IUser User { get { return Context.User; } }

        public IDiscordClient Client { get { return Context.Client; } }

        public IInteractionContext Context { get; }

        public OculusVariables(IInteractionContext ctx)
        {
            this.Context = ctx;
        }
    }
}
