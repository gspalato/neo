using Discord;

namespace Neo.Core.Structures
{
    public class NeoVariables
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

        public NeoVariables(IInteractionContext ctx, IServiceProvider services)
        {
            this.Context = ctx;
            this.Services = services;
        }
    }
}
