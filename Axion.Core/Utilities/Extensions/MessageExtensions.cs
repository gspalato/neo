using Axion.Core.Structures.Interactivity;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Utilities.Extensions
{
    public static class MessageExtensions
    {
        public static async Task<LazyObject<SocketReaction>> AwaitReaction(this IUserMessage message, DiscordSocketClient client,
            Func<SocketReaction, bool> filter) =>
            await message.GetReactionAwaiter(client, filter).Wait();

        public static async Task<LazyObject<SocketReaction>> AwaitNextReaction(this IUserMessage message, DiscordSocketClient client) =>
            await message.GetReactionAwaiter(client, r => true).Wait();

        public static ReactionAwaiter GetReactionAwaiter(this IUserMessage message, DiscordSocketClient client,
            Func<SocketReaction, bool> filter) =>
            new ReactionAwaiter(client, message, filter);
    }
}
