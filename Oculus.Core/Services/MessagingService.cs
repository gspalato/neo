using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Oculus.Common.Structures;
using System.Threading.Tasks;

namespace Oculus.Core.Services
{
    public enum MessageType
    {
        Default,
        Info,
        Diagnostic
    }

    public interface IMessagingService
    {

    }

    public class MessagingService : ServiceBase, IMessagingService
    {
        public readonly IDiscordClient client;
        public readonly DiscordWebhookClient webhookClient;

        public MessagingService(DiscordSocketClient client, DiscordWebhookClient webhook)
        {
            this.client = client;
            webhookClient = webhook;
        }

        public async Task SendAs(MessageType type, ITextChannel channel, string content = "", Embed embed = default)
        {
            switch (type)
            {
                case MessageType.Default:
                    {
                        await webhookClient.SendMessageAsync(
                            text: content,
                            embeds: new[] { embed },
                            username: client.CurrentUser.Username
                        );
                    }
                    break;
            }
        }
    }
}
