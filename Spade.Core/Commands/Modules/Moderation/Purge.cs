using Spade.Core.Commands.ArgumentParsers;
using Spade.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Spade.Core.Commands.Modules.Moderation
{
	[Category(Category.Moderation)]
	[Description("Purge messages (with custom settings™️)")]
	[Group("purge", "clear")]
	public class Purge : SpadeModule
	{
		[Command]
		[RequireChannelUserPermissions(ChannelPermission.ManageMessages)]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[OverrideArgumentParser(typeof(UnixArgumentParser))]
		public async Task PurgeAsync(
			[Range(1, 100, true, true)]
			[Name("Count")]   int count,
			[Name("User")] IGuildUser user = null,
			[Name("Channel")] ITextChannel channel = null,
			[Name("Embeds")] bool embeds = false,
			[Name("After")] ulong? afterMessageId = null,
			[Name("Before")] ulong? beforeMessageId = null,
			[Name("Self")] bool self = false,
			[Name("BotOnly")] bool botsOnly = false,
			[Name("Silent")] bool silent = false)
		{
			if (afterMessageId is not null && beforeMessageId is not null)
			{
				var error = await SendErrorAsync("You can't use both --before and --after.");
				await Task.Delay(3000);
				await error.DeleteAsync();

				return;
			}

			var ch = channel ?? Context.Channel;

			var id = afterMessageId ?? beforeMessageId ?? Context.Message.Id;
			var direction = afterMessageId is not null ? Direction.After : Direction.Before;

			var request = await ch.GetMessagesAsync(id, direction, count).ElementAtOrDefaultAsync(1);
			if (!request.Any())
			{
				await Context.ReactAsync("❌");
				return;
			}

			var messageCount = 0;
			var filtered = request
				.Where(m =>
				{
					if (user is not null && m.Author.Id != user.Id)
						return false;

					if (embeds && !m.Embeds.Any())
						return false;

					if (botsOnly && !m.Author.IsBot)
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

			if (self)
				await Context.Message.DeleteAsync();

			await ch.DeleteMessagesAsync(messages).ConfigureAwait(false);

			if (silent)
				return;

			var sb = new StringBuilder();
			sb.AppendLine($"Deleted `{messageCount}` messages.");
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

		[Command]
		[RequireChannelUserPermissions(ChannelPermission.ManageMessages)]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		public async Task PurgeDefaultAsync(
			[Range(1, 100, true, true)]
			[Name("Count")] int count,
			[Name("User")] IGuildUser user = null)
		{
			var ch = Context.Channel;

			var request = await ch.GetMessagesAsync(count + 1).ElementAtOrDefaultAsync(1);
			if (!request.Any())
			{
				await Context.ReactAsync("❌");
				return;
			}

			var messageCount = 0;
			var filtered = request
				.Where(m =>
				{
					if (user is not null && m.Author.Id != user.Id)
						return false;

					if (m.Id == Context.Message.Id)
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

			await Context.Message.DeleteAsync();

			await ch.DeleteMessagesAsync(messages).ConfigureAwait(false);

			var sb = new StringBuilder();
			sb.AppendLine($"Deleted `{messageCount}` messages.");
			sb.AppendLine();

			List<string> deletedMessageUsers = new();

			foreach (var author in messages.GroupBy(b => b.Author.Id))
			{
				var u = await Context.Guild.GetUserAsync(author.Key);
				deletedMessageUsers.Add($"**{u?.ToString() ?? author.Key.ToString()}**: {author.Count()} messages");
			}

			var truncatedUsers = deletedMessageUsers.Take(3);

			sb.AppendJoin("\n", truncatedUsers);
			if (truncatedUsers.Count() < deletedMessageUsers.Count())
				sb.AppendLine("...");

			var ok = await SendOkAsync(sb.ToString());
			await Task.Delay(3000);
			await ok.DeleteAsync();
		}
	}
}
