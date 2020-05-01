using Axion.Core.Structures.Interactivity;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Utilities.Extensions
{
    public static class TextChannelExtensions
    {
        public static LazyObject<IMessage> AwaitMessage(this ITextChannel channel, DiscordSocketClient client,
            Func<IMessage, bool> filter) =>
            channel.GetMessageAwaiter(client, filter).Wait();

        public static LazyObject<IMessage> AwaitNextMessage(this ITextChannel channel, DiscordSocketClient client) =>
            channel.GetMessageAwaiter(client, m => true).Wait();

        public static MessageAwaiter GetMessageAwaiter(this ITextChannel channel, DiscordSocketClient client,
            Func<IMessage, bool> filter) =>
            new MessageAwaiter(client, channel, filter);
    }
}
