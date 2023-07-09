using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Neo.Libraries.Interactivity.Structures.Builders;
using Neo.Libraries.Interactivity.Structures.Contexts;

namespace Neo.Libraries.Interactivity
{
    using MenuCallback = Func<SocketMessageComponent, MenuContext, string, bool>;
    using PaginationCallback = Func<SocketMessageComponent, PaginationContext, string, bool>;
    using SelectionCallback = Func<SocketMessageComponent, SelectionContext, string, bool>;

    public class InteractivityService
    {
        private DiscordSocketClient Client { get; }

        private Dictionary<string, Structures.InteractivitySession> Interactivities { get; } = new();

        private List<SelectionContext> ButtonContexts { get; } = new();
        private Dictionary<string, SelectionCallback> ButtonCallbacks { get; } = new();

        private List<PaginationContext> PaginationContexts { get; } = new();
        private Dictionary<string, PaginationCallback> PaginationCallbacks { get; } = new();

        private List<MenuContext> MenuContexts { get; } = new();
        private Dictionary<string, MenuCallback> MenuCallbacks { get; } = new();


        public InteractivityService(DiscordSocketClient client)
        {
            Client = client;
        }

        public void Initialize()
        {
            Client.ButtonExecuted += HandleButton;
            Client.SelectMenuExecuted += HandleSelectMenu;
        }

        public Tuple<Embed?, ComponentBuilder> UseInteractivity(InteractivityBuilder builder)
        {
            var interactivity = builder.Build();

            if (Interactivities.ContainsKey(interactivity.Id))
                return default!;

            Interactivities.Add(interactivity.Id, interactivity);
            var components = interactivity.Components;

            Embed? initialPaginationPage = interactivity.PaginationContext?.Pages[interactivity.PaginationContext.CurrentPage];

            if (interactivity.PaginationContext is not null)
            {
                Console.WriteLine($"Registering pagination from Interactivity {interactivity.Id}");
                PaginationContexts.Add(interactivity.PaginationContext);

                foreach (var button in interactivity.PaginationContext.Buttons)
                    RegisterPaginationButtonHandler(button.Component.CustomId, button.Callback);
            }

            foreach (var menuContext in interactivity.MenuContexts)
            {
                Console.WriteLine($"Registering select menu with {menuContext.Options.Count} options from Interactivity {interactivity.Id}");
                MenuContexts.Add(menuContext);

                foreach (var option in menuContext.Options)
                    RegisterSelectMenuOptionHandler(option.Component.Value, option.Callback);
            }

            foreach (var buttonContext in interactivity.ButtonContexts)
            {
                Console.WriteLine($"Registering button row with {buttonContext.Buttons.Count} buttons from Interactivity {interactivity.Id}");
                ButtonContexts.Add(buttonContext);

                foreach (var button in buttonContext.Buttons)
                    RegisterButtonHandler(button.Component.CustomId, button.Callback);
            }

            return new Tuple<Embed?, ComponentBuilder>(initialPaginationPage, components);
        }

        public bool UnregisterInteractivity(string id)
        {
            if (!Interactivities.ContainsKey(id))
                return false;

            var interactivity = Interactivities[id];
            Interactivities.Remove(id);

            if (interactivity.PaginationContext is not null)
            {
                PaginationContexts.Remove(interactivity.PaginationContext);
                interactivity.PaginationContext.Buttons.ForEach(x => PaginationCallbacks.Remove(x.Component.CustomId));
            }

            foreach (var menuContext in interactivity.MenuContexts)
            {
                MenuContexts.Remove(menuContext);
                menuContext.Options.ForEach(x => MenuCallbacks.Remove(x.Component.Value));
            }

            foreach (var buttonContext in interactivity.ButtonContexts)
            {
                ButtonContexts.Remove(buttonContext);
                buttonContext.Buttons.ForEach(x => ButtonCallbacks.Remove(x.Component.CustomId));
            }

            return true;
        }

        public bool RegisterButtonHandler(string id, Func<SocketMessageComponent, SelectionContext, string, bool> handler)
        {
            if (ButtonCallbacks.ContainsKey(id))
                return false;

            ButtonCallbacks.Add(id, handler);
            return true;
        }

        public bool UnregisterButtonHandler(string id)
        {
            if (ButtonCallbacks.ContainsKey(id))
            {
                ButtonCallbacks.Remove(id);
                return true;
            }

            return false;
        }

        public bool RegisterPaginationButtonHandler(string id, Func<SocketMessageComponent, PaginationContext, string, bool> handler)
        {
            if (PaginationCallbacks.ContainsKey(id))
                return false;


            PaginationCallbacks.Add(id, handler);
            return true;
        }

        public bool UnregisterPaginationButtonHandler(string id)
        {
            if (PaginationCallbacks.ContainsKey(id))
            {
                PaginationCallbacks.Remove(id);
                return true;
            }

            return false;
        }

        public bool RegisterSelectMenuOptionHandler(string id, Func<SocketMessageComponent, MenuContext, string, bool> handler)
        {
            if (MenuCallbacks.ContainsKey(id))
                return false;

            MenuCallbacks.Add(id, handler);
            return true;
        }

        public bool UnregisterSelectMenuOptionHandler(string id)
        {
            if (MenuCallbacks.ContainsKey(id))
            {
                MenuCallbacks.Remove(id);
                return true;
            }

            return false;
        }

        private Task HandleButton(SocketMessageComponent interaction)
        {
            _ = Task.Run(async () =>
            {
                string id = interaction.Data.CustomId;

                if (PaginationCallbacks.ContainsKey(id))
                {
                    var callback = PaginationCallbacks[id];

                    var paginationContext = PaginationContexts.First(x => x.Buttons.Any(y => y.Component.CustomId == id));
                    var shouldDelete = callback.Invoke(interaction, paginationContext, id);

                    if (shouldDelete)
                    {
                        await interaction.DeferAsync();
                        await interaction.DeleteOriginalResponseAsync();
                        UnregisterInteractivity(paginationContext.InteractivitySessionId);
                    }
                }
                else if (ButtonCallbacks.ContainsKey(id))
                {
                    var callback = ButtonCallbacks[id];

                    var selectionContext = ButtonContexts.First(x => x.Buttons.Any(y => y.Component.CustomId == id));
                    var shouldDelete = callback.Invoke(interaction, selectionContext, id);

                    if (shouldDelete)
                    {
                        await interaction.DeferAsync();
                        await interaction.DeleteOriginalResponseAsync();
                        UnregisterInteractivity(selectionContext.InteractivitySessionId);
                    }
                }
                else
                {
                    await interaction.FollowupAsync("Unknown button.");
                    return;
                }
            });

            return Task.CompletedTask;
        }

        private Task HandleSelectMenu(SocketMessageComponent interaction)
        {
            _ = Task.Run(async () =>
            {
                var selectedIds = interaction.Data.Values;
                Console.WriteLine(string.Join(", ", selectedIds));

                if (!selectedIds.All(id => MenuCallbacks.ContainsKey(id)))
                {
                    await interaction.FollowupAsync("Unknown option.");
                    return;
                }

                var menuContext = MenuContexts.First(x => x.Menu.Options.Any(y => interaction.Data.Values.Any(z => y.Value == z)));
                menuContext.SelectedOptions = selectedIds.ToList();
                foreach (var option in menuContext.Menu.Options)
                {
                    if (!selectedIds.Contains(option.Value))
                        continue;

                    var callback = MenuCallbacks.First(x => x.Key == option.Value).Value;
                    var shouldDelete = callback.Invoke(interaction, menuContext, option.Value);

                    if (shouldDelete)
                    {
                        await interaction.DeferAsync();
                        await interaction.DeleteOriginalResponseAsync();
                        MenuContexts.Remove(menuContext);
                        menuContext.Menu.Options.ForEach(x => MenuCallbacks.Remove(x.Value));
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}