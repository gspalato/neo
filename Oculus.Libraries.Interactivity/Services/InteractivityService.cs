using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Oculus.Libraries.Interactivity.Structures.Builders;
using Oculus.Libraries.Interactivity.Structures.Contexts;

namespace Oculus.Libraries.Interactivity
{
    public class InteractivityService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;

        private readonly List<SelectionContext> _buttonContexts = new();
        private readonly Dictionary<string, Func<SocketMessageComponent, SelectionContext, bool>> _buttonHandlers = new();

        private readonly List<PaginationContext> _paginationContexts = new();
        private readonly Dictionary<string, Func<SocketMessageComponent, PaginationContext, bool>> _paginationButtonHandlers = new();



        public InteractivityService(DiscordSocketClient client, InteractionService commands)
        {
            _client = client;
            _commands = commands;
        }

        public void Initialize()
        {
            _client.ButtonExecuted += HandleButton;
        }

        public ComponentBuilder UseSelection(SelectionBuilder builder)
        {
            var buttonRow = builder.Build();
            _buttonContexts.Add(buttonRow);

            var components = new ComponentBuilder();
            foreach (var tuple in buttonRow.Buttons)
            {
                components.WithButton(tuple.Component);
                RegisterButtonHandler(tuple.Component.CustomId, tuple.Callback);
            }

            return components;
        }

        public Tuple<Embed, ComponentBuilder> UsePagination(PaginationBuilder builder)
        {
            var pages = builder.Pages;

            var pagination = builder.Build();
            _paginationContexts.Add(pagination);

            var embed = pages[pagination.CurrentPage];
            var components = new ComponentBuilder();
            foreach (var tuple in pagination.Buttons)
            {
                components.WithButton(tuple.Component);
                RegisterPaginationButtonHandler(tuple.Component.CustomId, tuple.Callback);
            }

            return new Tuple<Embed, ComponentBuilder>(embed, components);
        }

        public bool RegisterButtonHandler(string id, Func<SocketMessageComponent, SelectionContext, bool> handler)
        {
            if (_buttonHandlers.ContainsKey(id))
                return false;

            _buttonHandlers.Add(id, handler);
            return true;
        }

        public bool UnregisterButtonHandler(string id)
        {
            if (_buttonHandlers.ContainsKey(id))
            {
                _buttonHandlers.Remove(id);
                return true;
            }

            return false;
        }

        public bool RegisterPaginationButtonHandler(string id, Func<SocketMessageComponent, PaginationContext, bool> handler)
        {
            if (_paginationButtonHandlers.ContainsKey(id))
                return false;


            _paginationButtonHandlers.Add(id, handler);
            return true;
        }

        public bool UnregisterPaginationButtonHandler(string id)
        {
            if (_paginationButtonHandlers.ContainsKey(id))
            {
                _paginationButtonHandlers.Remove(id);
                return true;
            }

            return false;
        }

        private Task HandleButton(SocketMessageComponent interaction)
        {
            _ = Task.Run(async () => {
                string id = interaction.Data.CustomId;

                if (_paginationButtonHandlers.ContainsKey(id))
                {
                    var callback = _paginationButtonHandlers.First(x => x.Key == id).Value;

                    var paginationContext = _paginationContexts.First(x => x.Buttons.Any(y => y.Component.CustomId == id));
                    var shouldDelete = callback.Invoke(interaction, paginationContext);

                    if (shouldDelete)
                    {
                        await interaction.DeferAsync();
                        await interaction.DeleteOriginalResponseAsync();
                        _paginationContexts.Remove(paginationContext);
                        paginationContext.Buttons.ForEach(x => _paginationButtonHandlers.Remove(x.Component.CustomId));
                    }
                }
                else if (_buttonHandlers.ContainsKey(id))
                {
                    var callback = _buttonHandlers.First(x => x.Key == id).Value;

                    var buttonRowContext = _buttonContexts.First(x => x.Buttons.Any(y => y.Component.CustomId == id));
                    var shouldDelete = callback.Invoke(interaction, buttonRowContext);

                    if (shouldDelete)
                    {
                        await interaction.DeferAsync();
                        await interaction.DeleteOriginalResponseAsync();
                        _buttonContexts.Remove(buttonRowContext);
                        buttonRowContext.Buttons.ForEach(x => _buttonHandlers.Remove(x.Component.CustomId));
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
    }
}