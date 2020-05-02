using Axion.Core.Structures.Attributes;
using Axion.Core.Utilities;
using Axion.Core.Extensions;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Axion.Commands.Modules
{
	[Category("Moderation")]
	[Description("Le ban hammer")]
	public sealed class Moderation : AxionModule
	{
		[Command("ban")]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[RequireGuildBotPermissions(GuildPermission.BanMembers)]
		[RequireGuildUserPermissions(GuildPermission.BanMembers)]
		public async Task BanAsync(IGuildUser member, [Remainder] string reason = "Unspecified reason.")
		{
			var msg = await SendDefaultEmbedAsync("Confirmation",
				$"Are you sure you want to ban {member.Mention} for `{reason.TruncateAndEscape()}`");

			_ = msg.AddReactionsAsync(new[] { new Emoji("✅"), new Emoji("❌") });

			var lazyReaction = msg.AwaitReaction(Context.Client, (r) =>
				r.UserId == Context.Message.Author.Id && (new[] { "✅", "❌" }).Contains(r.Emote.Name));
			var result = await lazyReaction;

			if (!lazyReaction.IsCompleted || lazyReaction.IsCanceled)
			{
				var embed = CreateDefaultEmbed("Aborted", $"Reaction timedout.");
				await msg.ModifyAsync(props =>
					props.Embed = embed.Build());

				return;
			}

			switch (result.Emote.Name)
			{
				case "✅":
					{
						try
						{
							await member.BanAsync(reason: reason);

							var embed = CreateOkEmbed("Success", $"Banned {member.Mention} for `{reason.TruncateAndEscape()}`");
							await msg.ModifyAsync(props =>
								props.Embed = embed.Build());
						}
						catch
						{
							var embed = CreateErrorEmbed("Error", $"Couldn't ban {member.Mention}. Check if I have enough permissions.");
							await msg.ModifyAsync(props =>
								props.Embed = embed.Build());
						}
					}
					break;

				case "❌":
					{
						var embed = CreateErrorEmbed("Aborted", $"You can go away this time. Only this time.");
						await msg.ModifyAsync(props =>
							props.Embed = embed.Build());
					}
					break;
			}

			await msg.RemoveAllReactionsAsync();
		}

		[Command("kick")]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[RequireGuildBotPermissions(GuildPermission.KickMembers)]
		[RequireGuildUserPermissions(GuildPermission.KickMembers)]
		public async Task KickAsync(IGuildUser member, [Remainder] string reason = "Unspecified reason.")
		{
			var msg = await SendDefaultEmbedAsync("Confirmation",
				$"Are you sure you want to kick {member.Mention} for `{reason.TruncateAndEscape()}`");

			_ = msg.AddReactionsAsync(new[] { new Emoji("✅"), new Emoji("❌") });

			var lazyReaction = msg.AwaitReaction(Context.Client, (r) =>
				r.UserId == Context.Message.Author.Id && (new[] { "✅", "❌" }).Contains(r.Emote.Name));
			var result = await lazyReaction;

			if (!lazyReaction.IsCompleted || lazyReaction.IsCanceled)
			{
				var embed = CreateDefaultEmbed("Aborted", $"Reaction timedout.");
				await msg.ModifyAsync(props =>
					props.Embed = embed.Build());

				return;
			}

			switch (result.Emote.Name)
			{
				case "✅":
					{
						try
						{
							await member.KickAsync(reason: reason);

							var embed = CreateOkEmbed("Success", $"Kicked {member.Mention} for `{reason.TruncateAndEscape()}`");
							await msg.ModifyAsync(props =>
								props.Embed = embed.Build());
						}
						catch
						{
							var embed = CreateErrorEmbed("Error", $"Couldn't kick {member.Mention}. Check if I have enough permissions.");
							await msg.ModifyAsync(props =>
								props.Embed = embed.Build());
						}
					}
					break;

				case "❌":
					{
						var embed = CreateErrorEmbed("Aborted", $"You can go away this time. Only this time.");
						await msg.ModifyAsync(props =>
							props.Embed = embed.Build());
					}
					break;
			}

			await msg.RemoveAllReactionsAsync();
		}

		[Command("purge")]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[RequireChannelUserPermissions(ChannelPermission.ManageMessages)]
		[IgnoresExtraArguments]
		public async Task PurgeAsync(int number)
		{
			var channel = Context.Channel;
			var messages = await Context.Channel.GetMessagesAsync(number).ElementAtAsync(1);

			await channel.DeleteMessagesAsync(messages);

			var ok = await SendOkAsync($"Purged {messages.Count.ToString()} messages.");
			await Task.Delay(3000);
			await ok.DeleteAsync();
		}

		[Command("whois")]
		public async Task WhoIsAsync(IGuildUser target)
		{
			if (!(target is SocketGuildUser member))
				return;

			var escapedUsername = member.Username.Escape();
			var escapedNickname = member.Nickname?.Escape();

			var totalName = $"{escapedUsername} {(escapedNickname is null ? "" : $"({escapedNickname})")}";

			var rolesList =
				from role in member.Roles
				orderby role.Position descending
				select $"`{role.Name.Escape()}`";

			var embed = new EmbedBuilder()
				.WithAuthor(member.Username + "#" + member.Discriminator, member.GetAvatarUrl())
				.WithDescription(member.GetStatus())
				.WithColor(member.Id == Context.Client.CurrentUser.Id
					? new Color(0x2A8EF4)
					: member.GetHighestColor(new Color()))
				.AddField("Name", totalName, true)
				.AddField("Discriminator", "#" + member.Discriminator, true)
				.AddField("ID", member.Id.ToString(), true)
				.AddField("Bot?", member.IsBot ? "Yes" : "No", true)
				.AddField("Status", member.Status.ToString(), true)
				.AddField("Joined", member.JoinedAt?.ToUniversalTime().ToString().Substring(1, 18), true)
				.AddField("Roles", string.Join(", ", rolesList))
				.WithThumbnailUrl(member.GetAvatarUrl());

			await SendEmbedAsync(embed);
		}
	}
}