using Discord;
using Discord.Commands;
using Discord.Interactions;
using Oculus.Core.Services;
using RequireBotPermissionAttribute = Discord.Interactions.RequireBotPermissionAttribute;
using RequireUserPermissionAttribute = Discord.Interactions.RequireUserPermissionAttribute;

namespace Oculus.Core.Commands.Modules
{
    public class ModerationModule : InteractionModuleBase
    {
        private readonly ILoggingService _logger;

        public ModerationModule(ILoggingService logger)
        {
            _logger = logger;
        }

        [SlashCommand("purge", "Delete messages.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeDefaultAsync(
            [Name("Count")] int count,
            [Name("User")] IGuildUser? user = null)
        {
            if (Context.Channel is not ITextChannel ch)
                return;

            var request = await AsyncEnumerableExtensions.FlattenAsync(ch.GetMessagesAsync(count));
            if (!request.Any())
            {
                await RespondAsync("Failed to purge.", ephemeral: true);
                return;
            }

            var messageCount = 0;
            var filtered = request
                .Where(m =>
                {
                    if (user is not null && m.Author.Id != user.Id)
                        return false;

                    try
                    {
                        return m is IUserMessage && (DateTimeOffset.UtcNow - m.CreatedAt).TotalDays < 14;
                    }
                    finally
                    {
                        messageCount++;
                    }
                });

            var messages = filtered as IMessage[] ?? filtered.ToArray();

            await ch.DeleteMessagesAsync(messages).ConfigureAwait(false);

            /*
            var sb = new StringBuilder();
            sb.AppendLine($"Deleted `{messageCount}` messages.");
            sb.AppendLine();

            List<string> deletedMessageUsers = new List<string>();

            foreach (var author in messages.GroupBy(b => b.Author.Id))
            {
                var u = await Context.Guild.GetUserAsync(author.Key);
                deletedMessageUsers.Add($"**{u?.ToString() ?? author.Key.ToString()}**: {author.Count()} messages");
            }

            var truncatedUsers = deletedMessageUsers.Take(3);

            sb.AppendJoin("\n", truncatedUsers);
            if (truncatedUsers.Count() < deletedMessageUsers.Count)
                sb.AppendLine("...");
            */

            await RespondAsync($"Purged {messageCount} messages.", ephemeral: true);
        }
    }
}
