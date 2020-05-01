using Axion.Structures.Attributes;
using Axion.Core.Structures.Interactivity;
using Axion.Utilities;
using Discord;
using Discord.WebSocket;
using Qmmands;
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
		public async Task BanAsync(IGuildUser member, string reason = "")
		{
			var msg = await SendDefaultEmbedAsync("Confirmation",
				$"Are you sure you want to ban {member.Mention} for `{reason.TruncateAndEscape()}`");

			await msg.AddReactionAsync(new Emoji("✅"));
			await msg.AddReactionAsync(new Emoji("❌"));

			var awaiter = new ReactionAwaiter(Context.Client, msg, (r) => r.UserId == Context.Message.Author.Id);
			var lazyReaction = await awaiter.Run();

			if (!lazyReaction.isCompleted || lazyReaction.isTimedout)
			{
				var embed = CreateDefaultEmbed("Aborted", $"Aborted ban because of timeout.");
				await msg.ModifyAsync(props =>
				{
					props.Embed = embed.Build();
				});

				return;
			}

			if (lazyReaction.Result.Emote.Name == "✅")
			{
				try
				{
					await member.BanAsync(reason: reason);

					var embed = CreateOkEmbed("Success", $"Banned {member.Mention} for `{reason.TruncateAndEscape()}`");
					await msg.ModifyAsync(props =>
					{
						props.Embed = embed.Build();
					});
				}
				catch
				{
					var embed = CreateErrorEmbed("Error", $"Couldn't ban {member.Mention}. Check if I have enough permissions.");
					await msg.ModifyAsync(props =>
					{
						props.Embed = embed.Build();
					});
				}
			}
			else if (lazyReaction.Result.Emote.Name == "❌")
			{
				var embed = CreateErrorEmbed("Aborted", $"You can go away this time. Only this time.");
				await msg.ModifyAsync(props =>
				{
					props.Embed = embed.Build();
				});
			}
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
			var member = (SocketGuildUser)target;

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