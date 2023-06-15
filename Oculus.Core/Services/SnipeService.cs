using Discord;
using Discord.WebSocket;

namespace Oculus.Core.Services
{
    public class SnipeService
    {
        private Dictionary<string, List<IMessage>> Snipes { get; } = new();

        public SnipeService(DiscordSocketClient client)
        {
            client.MessageDeleted += HandleMessageDeleteAsync;
        }


        public IMessage? GetLatestMessageFromChannel(ulong channelId)
        {
            var messages = Snipes.FirstOrDefault(x => x.Key == channelId.ToString()).Value;
            if (messages is null)
                return null;

            return messages.LastOrDefault();
        }

        private async Task HandleMessageDeleteAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
        {
        try
        {
            IMessageChannel channel = await cachedChannel.GetOrDownloadAsync();
            IMessage message = await cachedMessage.GetOrDownloadAsync();

            if (message is null || message.Author.IsBot || message.Author.IsWebhook)
                return;

            List<IMessage> messages;
            if (!Snipes.ContainsKey(channel.Id.ToString()))
            {
                messages = new List<IMessage>();
                Snipes.Add(channel.Id.ToString(), messages);
            }

            messages = Snipes.FirstOrDefault(x => x.Key == channel.Id.ToString()).Value;

            if (messages.Count >= 10)
                messages.RemoveAt(0);

            messages.Add(message);

            Console.WriteLine($"Sniped message from {message.Author.Username} in {channel.Name}: {message.Content}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.StackTrace);
        }
        }
    }
}