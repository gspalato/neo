using Discord;
using Discord.WebSocket;
using Muon.Kernel.Structures;
using Muon.Kernel.Utilities;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Muon.Commands
{
	[Category("Moderation")]
	[Description("Le ban hammer")]
	public sealed class Moderation : MuonModule
	{
		[Command("purge")]
		[IgnoresExtraArguments]
		public async Task PurgeAsync(int number)
		{
			ITextChannel channel = Context.Channel;
			IEnumerable<IMessage> messages = Context.Channel.GetMessagesAsync(number) as IEnumerable<IMessage>;

			await channel.DeleteMessagesAsync(messages);

			IMessage ok = await SendOkAsync($"Purged {messages.Count()} messages.");
			await Task.Delay(5000);
			await ok.DeleteAsync();
		}

		[Command("whois")]
		public async Task WhoIsAsync(IGuildUser target)
		{
			SocketGuildUser member = target as SocketGuildUser;

			string escapedUsername = member.Username.Escape();
			string escapedNickname = member.Nickname?.Escape();

			string totalName = $"{escapedUsername} {(escapedNickname is null ? "" : $"({escapedNickname})")}";

			string status = string.IsNullOrEmpty(member.GetStatus())
				? ""
				: $"> {member.GetStatus()}";

			var rolesList = member.Roles
				.OrderByDescending((role) => role.Position)
				.Select((role) => $"`{role.Name.Escape()}`");

			EmbedBuilder embed = new EmbedBuilder()
				.WithAuthor(member.Username + "#" + member.Discriminator, null, member.GetAvatarUrl())
				.WithDescription(status)
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