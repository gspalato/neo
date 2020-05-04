using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Axion.Core.Structures.Interactivity
{
    using Page = ValueTuple<string, EmbedBuilder>;
    using ReactionBehavior = ValueTuple<Emoji, Action<PaginatedContext>>;

    public class PaginatedMessageBuilder
    {
        public Page Template { get; private set; }
        public TimeSpan Timeout { get; private set; }

        private readonly List<ReactionBehavior> _callbacks = new List<ReactionBehavior>();
        private readonly List<Page> _pages = new List<Page>();
        private readonly List<IUser> _resposibles = new List<IUser>();

        public PaginatedMessageBuilder() { }

        public PaginatedMessageBuilder AddPage(EmbedBuilder builder)
        {
            _pages.Add(("", builder));

            return this;
        }

        public PaginatedMessageBuilder AddPages(List<EmbedBuilder> builders)
        {
            foreach (var builder in builders)
            {
                AddPage(builder);
            }

            return this;
        }

        public PaginatedMessageBuilder AddButton(Emoji emoji, Action<PaginatedContext> action)
        {
            _callbacks.Add((emoji, action));

            return this;
        }

        public PaginatedMessageBuilder WithDefaultButtons()
        {
            AddButton(new Emoji("⏪"),
                async (ctx) =>
                {
                    await ctx.PaginatedMessage.SkipToPageAsync(0);
                    await ctx.PaginatedMessage.Message.RemoveReactionAsync(ctx.Reaction.Emote, ctx.User);
                });

            AddButton(new Emoji("◀️"),
                async (ctx) =>
                {
                    var pm = ctx.PaginatedMessage;

                    await pm.Message.RemoveReactionAsync(ctx.Reaction.Emote, ctx.User);

                    if (pm.CurrentPage <= 0)
                        return;

                    await pm.SkipToPageAsync(pm.CurrentPage - 1);
                });

            AddButton(new Emoji("⏹️"),
                async (ctx) =>
                {
                    await ctx.PaginatedMessage.Message.DeleteAsync();
                    ctx.PaginatedMessage.Dispose();
                });

            AddButton(new Emoji("▶️"),
                async (ctx) =>
                {
                    var pm = ctx.PaginatedMessage;

                    await pm.Message.RemoveReactionAsync(ctx.Reaction.Emote, ctx.User);

                    if (pm.CurrentPage >= pm.Pages.Length - 1)
                        return;

                    await pm.SkipToPageAsync(pm.CurrentPage + 1);
                });

            AddButton(new Emoji("⏩"),
                async (ctx) =>
                {
                    var pm = ctx.PaginatedMessage;

                    await pm.Message.RemoveReactionAsync(ctx.Reaction.Emote, ctx.User);

                    if (pm.CurrentPage == pm.Pages.Length - 1)
                        return;

                    await pm.SkipToPageAsync(pm.Pages.Length - 1);
                });

            return this;
        }

        public PaginatedMessageBuilder WithResponsible(IUser user)
        {
            _resposibles.Add(user);

            return this;
        }

        public PaginatedMessageBuilder WithTemplate(string content, EmbedBuilder template)
        {
            Template = (content, template);

            return this;
        }

        public PaginatedMessageBuilder WithTimeout(TimeSpan timeout)
        {
            Timeout = timeout;

            return this;
        }

        public PaginatedMessage Build(DiscordSocketClient client)
        {
            List<Embed> builtPages = new List<Embed>();

            foreach (var tuple in _pages)
            {
                builtPages.Add(tuple.Item2.Build());
            }

            var paginated = new PaginatedMessage(client, _resposibles, builtPages.ToArray(), _callbacks, (int)Timeout.TotalMilliseconds);

            return paginated;
        }
    }
}
