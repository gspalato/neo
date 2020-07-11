using Discord;
using Discord.WebSocket;
using InteractivityNET;
using System;
using System.Threading.Tasks;

namespace Axion.Common.Extensions
{
    public static class MessageExtensions
    {
        public static Task<SocketReaction> AwaitReaction(this IUserMessage message, DiscordSocketClient client,
            Predicate<SocketReaction> filter) =>
            message.GetReactionAwaiter(client, filter).Wait();

        public static Task<SocketReaction> AwaitNextReaction(this IUserMessage message, DiscordSocketClient client) =>
            message.GetReactionAwaiter(client, r => true).Wait();

        public static ReactionAwaiter GetReactionAwaiter(this IUserMessage message, DiscordSocketClient client,
            Predicate<SocketReaction> filter) =>
            new ReactionAwaiter(client, message, filter);
    }
}
