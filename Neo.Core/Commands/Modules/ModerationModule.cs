using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Neo.Common.Utilities;
using Neo.Common.Utilities.Extensions;
using Neo.Core.Services;
using Neo.Libraries.Interactivity;
using Neo.Libraries.Interactivity.Structures.Builders;
using Neo.Libraries.Interactivity.Structures.Contexts;
using RequireBotPermissionAttribute = Discord.Interactions.RequireBotPermissionAttribute;
using RequireUserPermissionAttribute = Discord.Interactions.RequireUserPermissionAttribute;

namespace Neo.Core.Commands.Modules
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
            var displayReason = reason.Length >= 75 ? Format.Code(reason.TruncateAndSanitize(75)) : reason;

            var promptPage = new EmbedBuilder()
                .WithTitle("Confirmation")
                .WithDescription($"Are you sure you want to kick {member.Mention} for {displayReason}")
                .WithDefaultColor();

            var abortedPage = new EmbedBuilder()
                .WithTitle("Aborted")
                .WithDescription("Timed out.");

            var confirmButton = new ButtonBuilder()
                .WithLabel("Yes")
                .WithStyle(ButtonStyle.Secondary);

            var cancelButton = new ButtonBuilder()
                .WithLabel("No")
                .WithStyle(ButtonStyle.Primary);

            TaskCompletionSource<bool> tcs = new();

            var OnConfirm = (SocketMessageComponent interaction, SelectionContext context, string id) =>
            {
                Console.WriteLine("Confirm");
                tcs.TrySetResult(true);
                return true;
            };

            var OnCancel = (SocketMessageComponent interaction, SelectionContext context, string id) =>
            {
                Console.WriteLine("Cancel");
                tcs.TrySetResult(false);
                return true;
            };

            var buttonRowBuilder = new SelectionBuilder()
                .WithButton(confirmButton, OnConfirm)
                .WithButton(cancelButton, OnCancel);

            var interactivityBuilder = new InteractivityBuilder()
                .AddSelection(buttonRowBuilder);

            var (_, components) = _interactivity.UseInteractivity(interactivityBuilder);
            var reply = await ReplyAsync(embed: promptPage.Build(), components: components.Build());

            var confirmed = await tcs.Task;
            if (confirmed)
            {
                try
                {
                    await member.KickAsync(reason);

                    var embed = Utilities.CreateDefaultEmbed("Success", $"Kicked {member.Mention} for {displayReason}");
                    await reply.ModifyAsync(props =>
                        {
                            props.Embed = embed.Build();
                            props.Components = null;
                        }
                    );
                }
                catch
                {
                    var embed = Utilities.CreateDefaultEmbed("Error", $"Couldn't kick {member.Mention}. Check if I have enough permissions.", Color.Red);
                    await reply.ModifyAsync(props =>
                        {
                            props.Embed = embed.Build();
                            props.Components = null;
                        }
                    );
                }
            }
            else
            {
                var embed = Utilities.CreateDefaultEmbed("Aborted", "You can go away this time. Only this time.");
                await reply.ModifyAsync(props =>
                    {
                        props.Embed = embed.Build();
                        props.Components = null;
                    }
                );
            }
        }

        [SlashCommand("ban", "Ban a user.")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(IGuildUser member, string reason = "Unspecified reason.")
        {
            var displayReason = Format.Code(reason.Length >= 75 ? reason.TruncateAndSanitize(75) : reason);

            var promptPage = new EmbedBuilder()
                .WithTitle("Confirmation")
                .WithDescription($"Are you sure you want to ban {member.Mention} for {displayReason}")
                .WithDefaultColor();

            var abortedPage = new EmbedBuilder()
                .WithTitle("Aborted")
                .WithDescription("Timed out.");

            var confirmButton = new ButtonBuilder()
                .WithLabel("Yes")
                .WithStyle(ButtonStyle.Secondary);

            var cancelButton = new ButtonBuilder()
                .WithLabel("No")
                .WithStyle(ButtonStyle.Primary);

            TaskCompletionSource<bool> tcs = new();

            var OnConfirm = (SocketMessageComponent interaction, SelectionContext context, string id) =>
            {
                tcs.TrySetResult(true);
                return true;
            };

            var OnCancel = (SocketMessageComponent interaction, SelectionContext context, string id) =>
            {
                tcs.TrySetResult(false);
                return true;
            };

            var buttonRowBuilder = new SelectionBuilder()
                .WithButton(confirmButton, OnConfirm)
                .WithButton(cancelButton, OnCancel);

            var interactivityBuilder = new InteractivityBuilder()
                .AddSelection(buttonRowBuilder);

            var (_, components) = _interactivity.UseInteractivity(interactivityBuilder);
            var reply = await ReplyAsync(embed: promptPage.Build(), components: components.Build());

            var confirmed = await tcs.Task;
            if (confirmed)
            {
                try
                {
                    await member.BanAsync(reason: reason);

                    var embed = Utilities.CreateDefaultEmbed("Success", $"Banned {member.Mention} for {displayReason}");
                    await reply.ModifyAsync(props =>
                        {
                            props.Embed = embed.Build();
                            props.Components = null;
                        }
                    );
                }
                catch
                {
                    var embed = Utilities.CreateDefaultEmbed("Error", $"Couldn't ban {member.Mention}. Check if I have enough permissions.", Color.Red);
                    await reply.ModifyAsync(props =>
                        {
                            props.Embed = embed.Build();
                            props.Components = null;
                        }
                    );
                }
            }
            else
            {
                var embed = Utilities.CreateDefaultEmbed("Aborted", "You can go away this time. Only this time.");
                await reply.ModifyAsync(props =>
                    {
                        props.Embed = embed.Build();
                        props.Components = null;
                    }
                );
            }
        }

        [SlashCommand("unban", "Unban an user.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task UnbanAsync(string id)
        {
            var bans = await Context.Guild.GetBansAsync().FlattenAsync();

            IBan? ban = bans.FirstOrDefault(x => x.User.Id.ToString() == id);

            ulong numberId;
            try
            {
                numberId = Convert.ToUInt64(id);
            }
            catch
            {
                await ReplyAsync("Invalid ID.");
                return;
            }

            if (ban is not null)
            {
                Convert.ToUInt64(id);
                await Context.Guild.RemoveBanAsync(numberId);
                await ReplyAsync($"Unbanned {ban.User.Mention}.");
            }
            else
            {
                await ReplyAsync($"Couldn't unban {numberId}. Maybe this user wasn't banned before?");
            }
        }

        [SlashCommand("softban", "Softban an user.")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SoftbanAsync(IGuildUser member, string reason = "Unspecified reason.")
        {
            var displayReason = Format.Code(reason.Length >= 75 ? reason.TruncateAndSanitize(75) : reason);

            var promptPage = new EmbedBuilder()
                .WithTitle("Confirmation")
                .WithDescription($"Are you sure you want to softban {member.Mention} for {displayReason}")
                .WithDefaultColor();

            var abortedPage = new EmbedBuilder()
                .WithTitle("Aborted")
                .WithDescription("Timed out.");

            var confirmButton = new ButtonBuilder()
                .WithLabel("Yes")
                .WithStyle(ButtonStyle.Secondary);

            var cancelButton = new ButtonBuilder()
                .WithLabel("No")
                .WithStyle(ButtonStyle.Primary);

            TaskCompletionSource<bool> tcs = new();

            var OnConfirm = (SocketMessageComponent interaction, SelectionContext context, string id) =>
            {
                tcs.TrySetResult(true);
                return true;
            };

            var OnCancel = (SocketMessageComponent interaction, SelectionContext context, string id) =>
            {
                tcs.TrySetResult(false);
                return true;
            };

            var buttonRowBuilder = new SelectionBuilder()
                .WithButton(confirmButton, OnConfirm)
                .WithButton(cancelButton, OnCancel);

            var interactivityBuilder = new InteractivityBuilder()
                .AddSelection(buttonRowBuilder);

            var (_, components) = _interactivity.UseInteractivity(interactivityBuilder);
            var reply = await ReplyAsync(embed: promptPage.Build(), components: components.Build());

            var confirmed = await tcs.Task;
            if (confirmed)
            {
                try
                {
                    await member.BanAsync(reason: reason);
                    await Context.Guild.RemoveBanAsync(member.Id);

                    var embed = Utilities.CreateDefaultEmbed("Success", $"Softbanned {member.Mention} for {displayReason}");
                    await reply.ModifyAsync(props =>
                        {
                            props.Embed = embed.Build();
                            props.Components = null;
                        }
                    );
                }
                catch
                {
                    var embed = Utilities.CreateDefaultEmbed("Error", $"Couldn't softban {member.Mention}. Check if I have enough permissions.", Color.Red);
                    await reply.ModifyAsync(props =>
                        {
                            props.Embed = embed.Build();
                            props.Components = null;
                        }
                    );
                }
            }
            else
            {
                var embed = Utilities.CreateDefaultEmbed("Aborted", "You can go away this time. Only this time.");
                await reply.ModifyAsync(props =>
                    {
                        props.Embed = embed.Build();
                        props.Components = null;
                    }
                );
            }
        }
    }
}