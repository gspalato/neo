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

        private readonly List<ButtonContext> _buttonContexts = new();
        private readonly Dictionary<string, Func<SocketMessageComponent, ButtonContext, bool>> _buttonHandlers = new();

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
                Console.WriteLine($"'Added' button with ID: {tuple.Item1.CustomId}");
            }

            return new Tuple<Embed, ComponentBuilder>(embed, components);
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

        private async Task HandleButton(SocketMessageComponent interaction)
        {
            Console.WriteLine($"Handling button with ID: {interaction.Data.CustomId}");
            string id = interaction.Data.CustomId;
            Func<SocketMessageComponent, PaginationContext, bool> handler;

            if (_paginationButtonHandlers.ContainsKey(id))
                handler = _paginationButtonHandlers.First(x => x.Key == id).Value;
            else
            {
                await interaction.FollowupAsync("Unknown button.");
                return;
            }

            var pagination = _paginationContexts.First(x => x.Buttons.Any(y => y.Item1.CustomId == id));
            bool shouldDelete = handler.Invoke(interaction, pagination);

            if (shouldDelete)
            {
                await interaction.DeleteOriginalResponseAsync();
                _paginationContexts.Remove(pagination);
                pagination.Buttons.ForEach(x => _paginationButtonHandlers.Remove(x.Item1.CustomId));
            }
        }
    }
}