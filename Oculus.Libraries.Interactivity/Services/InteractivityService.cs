using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using Discord.Rest;
using Discord.Commands;
using Oculus.Libraries.Interactivity.Structures;
using System.Net.Sockets;

namespace Oculus.Libraries.Interactivity
{
    public class InteractivityService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;

        private readonly List<ButtonRowContext> _buttonContexts = new();
        private readonly Dictionary<string, Func<SocketMessageComponent, ButtonRowContext, bool>> _buttonHandlers = new();

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

        public ComponentBuilder UseButtonRow(ButtonRowBuilder builder)
        {
            var buttonRow = builder.Build();
            _buttonContexts.Add(buttonRow);

            var components = new ComponentBuilder();
            foreach (var tuple in buttonRow.Buttons)
            {
                components.WithButton(tuple.Item1);
                RegisterButtonHandler(tuple.Item1.CustomId, tuple.Item2);
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
                components.WithButton(tuple.Item1);
                RegisterPaginationButtonHandler(tuple.Item1.CustomId, tuple.Item2);
            }

            return new Tuple<Embed, ComponentBuilder>(embed, components);
        }

        public bool RegisterButtonHandler(string id, Func<SocketMessageComponent, ButtonRowContext, bool> handler)
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

                await interaction.DeferAsync();
                if (_paginationButtonHandlers.ContainsKey(id))
                {
                    Func<SocketMessageComponent, PaginationContext, bool> handler = _paginationButtonHandlers.First(x => x.Key == id).Value;

                    var paginationContext = _paginationContexts.First(x => x.Buttons.Any(y => y.Item1.CustomId == id));
                    bool shouldDelete = handler.Invoke(interaction, paginationContext);

                    if (shouldDelete)
                    {
                        await interaction.DeleteOriginalResponseAsync();
                        _paginationContexts.Remove(paginationContext);
                        paginationContext.Buttons.ForEach(x => _paginationButtonHandlers.Remove(x.Item1.CustomId));
                    }
                }
                else if (_buttonHandlers.ContainsKey(id))
                {
                    Func<SocketMessageComponent, ButtonRowContext, bool> handler = _buttonHandlers.First(x => x.Key == id).Value;

                    var buttonRowContext = _buttonContexts.First(x => x.Buttons.Any(y => y.Item1.CustomId == id));
                    bool shouldDelete = handler.Invoke(interaction, buttonRowContext);

                    if (shouldDelete)
                    {
                        await interaction.DeleteOriginalResponseAsync();
                        _buttonContexts.Remove(buttonRowContext);
                        buttonRowContext.Buttons.ForEach(x => _buttonHandlers.Remove(x.Item1.CustomId));
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