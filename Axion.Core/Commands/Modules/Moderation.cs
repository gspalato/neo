using Axion.Core.Commands.ArgumentParsers;
using Axion.Core.Extensions;
using Axion.Core.Structures.Attributes;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules
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
            var displayReason = reason.Length >= 75
                ? Format.Code(reason.EscapeCodeblock().Truncate(150), "")
                : Format.Code(Format.Sanitize(reason));

			var msg = await SendDefaultEmbedAsync("Confirmation",
				$"Are you sure you want to ban {member.Mention} for {displayReason}");

			var emojis = new IEmote[] {new Emoji("✅"), new Emoji("❌")};

			_ = msg.AddReactionsAsync(emojis);

			var lazyReaction = msg.AwaitReaction(Context.Client, r =>
				r.UserId == Context.Message.Author.Id && emojis.Contains(r.Emote));
			var result = await lazyReaction;

			if (!lazyReaction.IsCompleted || lazyReaction.IsCanceled)
			{
				var embed = CreateDefaultEmbed("Aborted", "Reaction timedout.");
				await msg.ModifyAsync(props => props.Embed = embed.Build());

				return;
			}

			var emoji = result.Emote.Name;
			switch (emoji)
			{
				case "✅":
					{
						try
						{
							await member.BanAsync(reason: reason);

                            var embed = CreateOkEmbed("Success",
								$"Banned {member.Mention} for {displayReason}");
							await msg.ModifyAsync(props => props.Embed = embed.Build());
						}
						catch
						{
							var embed = CreateErrorEmbed("Error",
								$"Couldn't ban {member.Mention}. Check if I have enough permissions.");
							await msg.ModifyAsync(props => props.Embed = embed.Build());
						}
					}
					break;

				default:
					{
						var embed = CreateErrorEmbed("Aborted", $"You can go away this time. Only this time.");
						await msg.ModifyAsync(props => props.Embed = embed.Build());
					}
					break;

			}

			await msg.RemoveAllReactionsAsync();
		}
		[Command("softban")]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[RequireGuildBotPermissions(GuildPermission.BanMembers)]
		[RequireGuildUserPermissions(GuildPermission.BanMembers)]
		public async Task SoftbanAsync(IGuildUser member, [Remainder] string reason = "Unspecified reason.")
		{
            var displayReason = reason.Length >= 75
                ? Format.Code(reason.EscapeCodeblock().Truncate(150), "")
                : Format.Code(Format.Sanitize(reason));

			var msg = await SendDefaultEmbedAsync("Confirmation",
				$"Are you sure you want to softban {member.Mention} for {displayReason}");

			var emojis = new IEmote[] { new Emoji("✅"), new Emoji("❌") };

			_ = msg.AddReactionsAsync(emojis);

			var lazyReaction = msg.AwaitReaction(Context.Client, r =>
				r.UserId == Context.Message.Author.Id && emojis.Contains(r.Emote));
			var result = await lazyReaction;

			if (!lazyReaction.IsCompleted || lazyReaction.IsCanceled)
			{
				var embed = CreateDefaultEmbed("Aborted", "Reaction timedout.");
				await msg.ModifyAsync(props => props.Embed = embed.Build());

				return;
			}

			var emoji = result.Emote.Name;
			switch (emoji)
			{
				case "✅":
					{
						try
						{
							await member.BanAsync(reason: reason);
							await Context.Guild.RemoveBanAsync(member);

                            var embed = CreateOkEmbed("Success",
								$"Softbanned {member.Mention} for {displayReason}");
							await msg.ModifyAsync(props =>
								props.Embed = embed.Build());
						}
						catch
						{
							var embed = CreateErrorEmbed("Error",
								$"Couldn't softban {member.Mention}. Check if I have enough permissions.");
							await msg.ModifyAsync(props => props.Embed = embed.Build());
						}
					}
					break;

				default:
					{
						var embed = CreateErrorEmbed("Error",
							$"Couldn't softban {member.Mention}. Check if I have enough permissions.");
						await msg.ModifyAsync(props => props.Embed = embed.Build());
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
            var displayReason = reason.Length >= 75
                ? Format.Code(reason.EscapeCodeblock().Truncate(150), "")
                : Format.Code(Format.Sanitize(reason));

			var msg = await SendDefaultEmbedAsync("Confirmation",
				$"Are you sure you want to kick {member.Mention} for {displayReason}");

			var emojis = new IEmote[] { new Emoji("✅"), new Emoji("❌") };

			_ = msg.AddReactionsAsync(emojis);

			var lazyReaction = msg.AwaitReaction(Context.Client, r =>
				r.UserId == Context.Message.Author.Id && emojis.Contains(r.Emote));
			var result = await lazyReaction;

			if (!lazyReaction.IsCompleted || lazyReaction.IsCanceled)
			{
				var embed = CreateDefaultEmbed("Aborted", $"Reaction timedout.");
				await msg.ModifyAsync(props => props.Embed = embed.Build());
				return;
			}

			var emoji = result.Emote.Name;
			switch (emoji)
			{
				case "✅":
					{
						try
						{
							await member.KickAsync(reason);

                            var embed = CreateOkEmbed("Success", $"Kicked {member.Mention} for {displayReason}");
							await msg.ModifyAsync(props => props.Embed = embed.Build());
						}
						catch
						{
							var embed = CreateErrorEmbed("Error", $"Couldn't kick {member.Mention}. Check if I have enough permissions.");
							await msg.ModifyAsync(props => props.Embed = embed.Build());
						}
					}
					break;

				default:
					{
						var embed = CreateErrorEmbed("Aborted", "You can go away this time. Only this time.");
						await msg.ModifyAsync(props => props.Embed = embed.Build());
					}
					break;
			}

			await msg.RemoveAllReactionsAsync();
		}

		[Command("purge", "clear")]
		[Description("Purge messages (with custom settings™️)")]
		[RequireChannelUserPermissions(ChannelPermission.ManageMessages)]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[OverrideArgumentParser(typeof(UnixArgumentParser))]
		public async Task PurgeAsync(
			[Name("Count"), Range(1, 100, true, true)] int count,
			[Name("User")]    IGuildUser user = null,
			[Name("Channel")] ITextChannel channel = null,
			[Name("Embeds")]  bool embeds = false,
			[Name("After")]   ulong? afterMessageId = null,
			[Name("Before")]  ulong? beforeMessageId = null,
			[Name("Self")]    bool self = false,
			[Name("BotOnly")] bool botsOnly = false,
			[Name("Silent")]  bool silent = false)
		{
			if (afterMessageId != null && beforeMessageId != null)
			{
				var error = await SendErrorAsync("You can't use both --before and --after.");
				await Task.Delay(3000);
				await error.DeleteAsync();

				return;
			}

			var ch = channel ?? Context.Channel;

			var id = afterMessageId ?? beforeMessageId ?? Context.Message.Id;
			var direction = afterMessageId != null ? Direction.After : Direction.Before;

			var request = await ch.GetMessagesAsync(id, direction, count).ElementAtOrDefaultAsync(1);
			if (request.Any())
				await Context.ReactAsync("❌");

			var messages = request
				.Where(m =>
				{
					if (user != null && m.Author.Id != user.Id)
						return false;

					if (embeds && !m.Embeds.Any())
						return false;

					if (botsOnly && !m.Author.IsBot)
						return false;

					return m is IUserMessage && (DateTimeOffset.UtcNow - m.CreatedAt).TotalDays < 14;
				});

			if (self)
				await Context.Message.DeleteAsync();

			await ch.DeleteMessagesAsync(messages).ConfigureAwait(false);

			if (silent)
				return;

			var sb = new StringBuilder();
			sb.AppendLine($"Deleted `{messages.Count()}` messages.");
			sb.AppendLine();

			foreach (var author in messages.GroupBy(b => b.Author.Id))
			{
				var u = await Context.Guild.GetUserAsync(author.Key);
				sb.AppendLine($"**{u?.ToString() ?? author.Key.ToString()}**: {author.Count()} messages");
			}

			var ok = await SendOkAsync(sb.ToString());
			await Task.Delay(3000);
			await ok.DeleteAsync();
		}

		[Command("whois")]
		public async Task WhoIsAsync(IGuildUser target)
		{
			if (!(target is SocketGuildUser member))
				return;

			var escapedUsername = Format.Sanitize(member.Username);
			var escapedNickname = Format.Sanitize(member.Nickname);

			var totalName = $"{escapedUsername} {(escapedNickname is null ? "" : $"({escapedNickname})")}";

			var rolesList = from role in member.Roles
							orderby role.Position descending
							select $"`{Format.Sanitize(role.Name)}`";

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