using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Oculus.Common.Utilities;
using Oculus.Common.Utilities.Extensions;
using Oculus.Core.Services;
using Oculus.Libraries.Interactivity;
using Oculus.Libraries.Interactivity.Structures;
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

            bool OnConfirm(SocketMessageComponent interaction, ButtonRowContext context)
            {
                tcs.TrySetResult(true);
                return true;
            }

            bool OnCancel(SocketMessageComponent interaction, ButtonRowContext context)
            {
                tcs.TrySetResult(false);
                return true;
            }

            var buttonRowBuilder = new ButtonRowBuilder()
                .WithButton(confirmButton, OnConfirm)
                .WithButton(cancelButton, OnCancel);

            var components = _interactivity.UseButtonRow(buttonRowBuilder);
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

        [SlashCommand("Ban", "Ban a user.")]
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

            bool OnConfirm(SocketMessageComponent interaction, ButtonRowContext context)
            {
                tcs.TrySetResult(true);
                return true;
            }

            bool OnCancel(SocketMessageComponent interaction, ButtonRowContext context)
            {
                tcs.TrySetResult(false);
                return true;
            }

            var buttonRowBuilder = new ButtonRowBuilder()
                .WithButton(confirmButton, OnConfirm)
                .WithButton(cancelButton, OnCancel);

            var components = _interactivity.UseButtonRow(buttonRowBuilder);
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
        public async Task UnbanAsync(ulong id)
        {
            var bans = await Context.Guild.GetBansAsync().FlattenAsync();

            IBan? ban;
            try
            {
                ban = bans.First(x => x.User.Id == id);
            }
            catch
            {
                ban = null;
            }

            if (ban is not null)
            {
                await Context.Guild.RemoveBanAsync(id);
                await ReplyAsync($"Unbanned {ban.User.Mention}.");
            }
            else
            {
                await ReplyAsync($"Couldn't unban {id}. Maybe this user wasn't banned before?");
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

            bool OnConfirm(SocketMessageComponent interaction, ButtonRowContext context)
            {
                tcs.TrySetResult(true);
                return true;
            }

            bool OnCancel(SocketMessageComponent interaction, ButtonRowContext context)
            {
                tcs.TrySetResult(false);
                return true;
            }

            var buttonRowBuilder = new ButtonRowBuilder()
                .WithButton(confirmButton, OnConfirm)
                .WithButton(cancelButton, OnCancel);

            var components = _interactivity.UseButtonRow(buttonRowBuilder);
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