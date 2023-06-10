using Discord;
using Discord.Commands;
using Discord.Interactions;
using Oculus.Common.Utilities;
using Oculus.Common.Utilities.Extensions;
using Oculus.Core.Services;
using Oculus.Libraries.Interactivity;

using RequireBotPermissionAttribute = Discord.Interactions.RequireBotPermissionAttribute;
using RequireUserPermissionAttribute = Discord.Interactions.RequireUserPermissionAttribute;

namespace Oculus.Core.Commands.Modules
{
    public class ModerationModule : InteractionModuleBase
    {
        private readonly InteractivityService _interactivity;

        private readonly ILoggingService _logger;

        public ModerationModule(InteractivityService interactivity, ILoggingService logger)
        {
            _interactivity = interactivity;
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

            await RespondAsync($"Purged {messageCount} messages.", ephemeral: true);
        }

        [SlashCommand("kick", "Kick a user.")]
		[RequireBotPermission(ChannelPermission.ManageMessages)]
		[RequireBotPermission(GuildPermission.KickMembers)]
		[RequireUserPermission(GuildPermission.KickMembers)]
		public async Task KickAsync(IGuildUser member, string reason = "Unspecified reason.")
		{
            /*
			var displayReason = reason.Length >= 75 ? Format.Code(reason.TruncateAndSanitize(75)) : reason;

            var promptPage = new PageBuilder()
                .WithTitle("Confirmation")
                .WithDescription($"Are you sure you want to kick {member.Mention} for {displayReason}")
                .WithColor(Color.Orange);

            var abortedPage = new PageBuilder()
                .WithTitle("Aborted")
                .WithDescription("Timed out.");

            var selection = new EmoteSelectionBuilder()
                .AddUser(Context.User)
                .AddOption(new Emoji("✅"))
                .AddOption(new Emoji("❌"))
                .WithTimeoutPage(abortedPage)
                .WithInputType(InputType.Buttons)
                .WithDeletion(DeletionOptions.None)
                .WithSelectionPage(promptPage);

            var result = await _interactive.SendSelectionAsync(selection.Build(), Context.Channel, TimeSpan.FromSeconds(30));
            var message = result.Message;
            var value = result.Value!;

			switch (value.Name)
			{
				case "✅":
					{
						try
						{
							await member.KickAsync(reason);

							var embed = Utilities.CreateDefaultEmbed("Success", $"Kicked {member.Mention} for {displayReason}");
							await message.ModifyAsync(props =>
                                {
                                    props.Embed = embed.Build();
                                    props.Components = null;
                                }
                            );
						}
						catch
						{
							var embed = Utilities.CreateDefaultEmbed("Error", $"Couldn't kick {member.Mention}. Check if I have enough permissions.");
							await message.ModifyAsync(props =>
                                {
                                    props.Embed = embed.Build();
                                    props.Components = null;
                                }
                            );
						}
					}
					break;

				default:
					{
						var embed = Utilities.CreateDefaultEmbed("Aborted", "You can go away this time. Only this time.");
						await message.ModifyAsync(props =>
                            {
                                props.Embed = embed.Build();
                                props.Components = null;
                            }
                        );
					}
					break;
			}
            */
		}
    }
}
