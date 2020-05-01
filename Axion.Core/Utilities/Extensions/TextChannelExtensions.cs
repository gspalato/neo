using Axion.Core.Structures.Interactivity;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Utilities.Extensions
{
    public static class TextChannelExtensions
    {
        public static async Task<LazyObject<IMessage>> AwaitMessage(this ITextChannel channel, DiscordSocketClient client,
            Func<IMessage, bool> filter) =>
            await channel.GetMessageAwaiter(client, filter).Wait();

        public static async Task<LazyObject<IMessage>> AwaitNextMessage(this ITextChannel channel, DiscordSocketClient client) =>
            await channel.GetMessageAwaiter(client, m => true).Wait();

        public static MessageAwaiter GetMessageAwaiter(this ITextChannel channel, DiscordSocketClient client,
            Func<IMessage, bool> filter) =>
            new MessageAwaiter(client, channel, filter);
    }
}
