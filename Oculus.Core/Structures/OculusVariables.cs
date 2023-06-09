using Discord;

namespace Oculus.Core.Structures
{
    public class OculusVariables
    {
        public IServiceProvider Services { get; }

        public IDiscordInteraction Interaction
        {
            get => Context.Interaction;
        }

        public ITextChannel Channel
        {
            get => (ITextChannel)Context.Channel;
        }

        public IGuild Guild
        {
            get => Channel.Guild;
        }
        
        public IUser User
        {
            get => Context.User;
        }

        public IDiscordClient Client
        {
            get => Context.Client;
        }

        public IInteractionContext Context { get; }

        public OculusVariables(IInteractionContext ctx, IServiceProvider services)
        {
            this.Context = ctx;
            this.Services = services;
        }
    }
}
