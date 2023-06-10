using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using Discord.Rest;
using Discord.Commands;
using Oculus.Libraries.Interactivity.Structures;

namespace Oculus.Libraries.Interactivity
{
    public class PaginationBuilder
    {
        public List<Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>> Buttons { get; private set; } = new();
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
                .WithLabel("First")
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(new Emoji("⏮️"));

            var previousPageButton = new ButtonBuilder()
                .WithLabel("Previous")
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(new Emoji("⏪"));

            var nextPageButton = new ButtonBuilder()
                .WithLabel("Next")
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(new Emoji("⏩"));

            var lastPageButton = new ButtonBuilder()
                .WithLabel("Last")
                .WithStyle(ButtonStyle.Secondary)
                .WithEmote(new Emoji("⏭️"));

            var stopButton = new ButtonBuilder()
                .WithLabel("Stop")
                .WithStyle(ButtonStyle.Danger)
                .WithEmote(new Emoji("🛑"));

            WithButton(firstPageButton, PaginationAction.First);
            WithButton(previousPageButton, PaginationAction.Previous);
            WithButton(nextPageButton, PaginationAction.Next);
            WithButton(lastPageButton, PaginationAction.Last);
            WithButton(stopButton, PaginationAction.Stop);
            
            return this;
        }

        public PaginationBuilder WithButton(ButtonBuilder button, Func<SocketMessageComponent, PaginationContext, bool> handler)
        {
            Buttons.Add(new Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>(button, handler));
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

                    Buttons.Add(new Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>(button, OnClick));
                }
                break;

                case PaginationAction.Previous:
                {
                    static bool OnClick(SocketMessageComponent interaction, PaginationContext pagination)
                    {
                        if (!pagination.UserIds.Contains(interaction.User.Id))
                            return false;

                        pagination.CurrentPage = pagination.CurrentPage == 0 ? 0 : pagination.CurrentPage - 1;
                        interaction.UpdateAsync(m => m.Embed = pagination.Pages[pagination.CurrentPage]);

                        return false;
                    };

                    Buttons.Add(new Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>(button, OnClick));
                }
                break;

                case PaginationAction.Next:
                {
                    static bool OnClick(SocketMessageComponent interaction, PaginationContext pagination)
                    {
                        if (!pagination.UserIds.Contains(interaction.User.Id))
                            return false;

                        pagination.CurrentPage = pagination.CurrentPage == pagination.Pages.Count - 1 ? pagination.Pages.Count - 1 : pagination.CurrentPage + 1;
                        interaction.UpdateAsync(m => m.Embed = pagination.Pages[pagination.CurrentPage]);

                        return false;
                    };

                    Buttons.Add(new Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>(button, OnClick));
                }
                break;

                case PaginationAction.Last:
                {
                    static bool OnClick(SocketMessageComponent interaction, PaginationContext pagination)
                    {
                        Console.WriteLine("Clicked on LAST.");
                        if (!pagination.UserIds.Contains(interaction.User.Id))
                            return false;

                        pagination.CurrentPage = pagination.Pages.Count - 1;
                        interaction.UpdateAsync(m => m.Embed = pagination.Pages[pagination.CurrentPage])
                            .ContinueWith(t => Console.WriteLine($"Set page to {pagination.CurrentPage}."));

                        return false;
                    };

                    Buttons.Add(new Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>(button, OnClick));
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

                    Buttons.Add(new Tuple<ButtonBuilder, Func<SocketMessageComponent, PaginationContext, bool>>(button, OnClick));
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