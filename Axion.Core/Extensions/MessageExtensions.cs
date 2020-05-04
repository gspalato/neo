using System;
using System.Threading.Tasks;
using Axion.Core.Structures.Interactivity;
using Discord;
using Discord.WebSocket;

namespace Axion.Core.Extensions
{
    public static class MessageExtensions
    {
        public static Task<SocketReaction> AwaitReaction(this IUserMessage message, DiscordSocketClient client,
            Func<SocketReaction, bool> filter) =>
            message.GetReactionAwaiter(client, filter).Wait();

        public static Task<SocketReaction> AwaitNextReaction(this IUserMessage message, DiscordSocketClient client) =>
            message.GetReactionAwaiter(client, r => true).Wait();

        public static ReactionAwaiter GetReactionAwaiter(this IUserMessage message, DiscordSocketClient client,
            Func<SocketReaction, bool> filter) =>
            new ReactionAwaiter(client, message, filter);
    }
}
