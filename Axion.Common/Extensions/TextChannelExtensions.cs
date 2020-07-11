using Discord;
using Discord.WebSocket;
using InteractivityNET;
using System;
using System.Threading.Tasks;

namespace Axion.Common.Extensions
{
    public static class TextChannelExtensions
    {
        public static Task<IMessage> AwaitMessage(this ITextChannel channel, DiscordSocketClient client,
            Predicate<IMessage> filter) =>
            channel.GetMessageAwaiter(client, filter).Wait();

        public static Task<IMessage> AwaitNextMessage(this ITextChannel channel, DiscordSocketClient client) =>
            channel.GetMessageAwaiter(client, m => m.Channel.Id == channel.Id).Wait();

        public static MessageAwaiter GetMessageAwaiter(this ITextChannel channel, DiscordSocketClient client,
            Predicate<IMessage> filter) =>
            new MessageAwaiter(client, channel, filter);
    }
}