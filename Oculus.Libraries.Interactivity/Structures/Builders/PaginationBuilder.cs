using Discord;
using Discord.WebSocket;
using Oculus.Libraries.Interactivity.Structures.Contexts;

namespace Oculus.Libraries.Interactivity.Structures.Builders
{
    public class PaginationBuilder
    {
        public List<InteractivityComponent<ButtonBuilder, PaginationContext>> Buttons { get; private set; } = new();
        public List<ulong> UserIds { get; private set; } = new();
        public List<Embed> Pages { get; private set; } = new();

        public PaginationBuilder() { }

        public PaginationBuilder WithPages(IEnumerable<Embed> pages)
        {
            Pages = pages.ToList();
            return this;
        }

        public PaginationBuilder WithUser(IUser user)
        {
            if (UserIds.Contains(user.Id))
                return this;

            UserIds.Add(user.Id);
            return this;
        }

        public PaginationBuilder WithUsers(IEnumerable<IUser> users)
        {
            foreach (var user in users)
                WithUser(user);

            return this;
        }

        public PaginationBuilder WithDefaultButtons()
        {
            var firstPageButton = new ButtonBuilder()
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(new Emoji("⏮️"));
            WithButton(firstPageButton, PaginationAction.First);

            var previousPageButton = new ButtonBuilder()
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(new Emoji("⏪"));
            WithButton(previousPageButton, PaginationAction.Previous);

            var nextPageButton = new ButtonBuilder()
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(new Emoji("⏩"));
            WithButton(nextPageButton, PaginationAction.Next);

            var lastPageButton = new ButtonBuilder()
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(new Emoji("⏭️"));
            WithButton(lastPageButton, PaginationAction.Last);

            var stopButton = new ButtonBuilder()
                .WithStyle(ButtonStyle.Danger)
                .WithEmote(new Emoji("🛑"));
            WithButton(stopButton, PaginationAction.Stop);
            
            return this;
        }

        public PaginationBuilder WithButton(ButtonBuilder button, Func<SocketMessageComponent, PaginationContext, bool> handler)
        {
            Buttons.Add(new InteractivityComponent<ButtonBuilder, PaginationContext>(button, handler));
            return this;
        }

        public PaginationBuilder WithButton(ButtonBuilder button, PaginationAction action)
        {
            switch (action)
            {
                case PaginationAction.First:
                {
                    static bool OnClick(SocketMessageComponent interaction, PaginationContext pagination)
                    {
                        if (!pagination.UserIds.Contains(interaction.User.Id))
                            return false;

                        pagination.CurrentPage = 0;
                        interaction.UpdateAsync(m => m.Embed = pagination.Pages[pagination.CurrentPage]);

                        return false;
                    };

                    Buttons.Add(new InteractivityComponent<ButtonBuilder, PaginationContext>(button, OnClick));
                }
                break;

                case PaginationAction.Previous:
                {
                    static bool OnClick(SocketMessageComponent interaction, PaginationContext pagination)
                    {
                        if (!pagination.UserIds.Contains(interaction.User.Id))
                            return false;

                        pagination.CurrentPage = pagination.CurrentPage == 0
                            ? 0
                            : pagination.CurrentPage - 1;

                        interaction.UpdateAsync(m => m.Embed = pagination.Pages[pagination.CurrentPage]);

                        return false;
                    };

                    Buttons.Add(new InteractivityComponent<ButtonBuilder, PaginationContext>(button, OnClick));
                }
                break;

                case PaginationAction.Next:
                {
                    static bool OnClick(SocketMessageComponent interaction, PaginationContext pagination)
                    {
                        if (!pagination.UserIds.Contains(interaction.User.Id))
                            return false;

                        pagination.CurrentPage = pagination.CurrentPage == pagination.Pages.Count - 1
                            ? pagination.Pages.Count - 1
                            : pagination.CurrentPage + 1;
                            
                        interaction.UpdateAsync(m => m.Embed = pagination.Pages[pagination.CurrentPage]);

                        return false;
                    };

                    Buttons.Add(new InteractivityComponent<ButtonBuilder, PaginationContext>(button, OnClick));
                }
                break;

                case PaginationAction.Last:
                {
                    static bool OnClick(SocketMessageComponent interaction, PaginationContext pagination)
                    {
                        if (!pagination.UserIds.Contains(interaction.User.Id))
                            return false;

                        pagination.CurrentPage = pagination.Pages.Count - 1;
                        interaction.UpdateAsync(m => m.Embed = pagination.Pages[pagination.CurrentPage]);

                        return false;
                    };

                    Buttons.Add(new InteractivityComponent<ButtonBuilder, PaginationContext>(button, OnClick));
                }
                break;

                case PaginationAction.Stop:
                {
                    static bool OnClick(SocketMessageComponent interaction, PaginationContext pagination)
                    {
                        if (!pagination.UserIds.Contains(interaction.User.Id))
                            return false;

                        return true;
                    };

                    Buttons.Add(new InteractivityComponent<ButtonBuilder, PaginationContext>(button, OnClick));
                }
                break;

                default:
                    return this;
            }

            return this;
        }

        public PaginationContext Build()
        {
            return new PaginationContext(Pages, UserIds, Buttons);
        }
    }
}