using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Arpa;
using Arpa.Utilities;


namespace Arpa.Commands
{
	public partial class Moderation : BaseCommandModule
	{
		[Command("whois")]
		public async Task WhoIsAsync(CommandContext ctx, DiscordMember member)
		{
			string escapedUsername = member.Username.Escape();
			string escapedNickname = member.Nickname != null
				? $"({member.Nickname.Escape()})"
				: "";

			var rolesList = member.Roles
				.OrderByDescending((role) => role.Position)
				.Select((role) => $"`{role.Name.Escape()}`");

			DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
				.WithAuthor(member.Username + "#" + member.Discriminator, null, member.AvatarUrl)
				.WithDescription($"> {this.GetStatus(member)}")
				.WithColor(member.Id == ctx.Client.CurrentUser.Id
					? new DiscordColor(0x2A8EF4)
					: this.GetHighestColor(member, new DiscordColor()))
				.AddField("Name", $"{escapedUsername} {escapedNickname}", true)
				.AddField("Discriminator", "#" + member.Discriminator, true)
				.AddField("ID", member.Id.ToString(), true)
				.AddField("Bot?", member.IsBot ? "Yes" : "No", true)
				.AddField("Status", member.Presence.Status.ToString(), true)
				.AddField("Joined", member.JoinedAt.ToUniversalTime().ToString().Substring(1, 18), true)
				.AddField("Roles", string.Join(", ", rolesList))
				.WithThumbnailUrl(member.AvatarUrl);

			await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
		}

		private string GetStatus(DiscordMember member)
		{
			DiscordActivity activity = member.Presence.Activity;
			ActivityType type = activity.ActivityType;

			switch (type)
			{
				case ActivityType.Custom:
					return $"{activity.CustomStatus.Emoji} {activity.CustomStatus.Name}";

				case ActivityType.Playing:
					return $"**Playing** {activity.Name}";

				case ActivityType.Watching:
					return $"**Watching** {activity.Name}";

				case ActivityType.ListeningTo:
					return $"**Listening to** {activity.Name}";

				case ActivityType.Streaming:
					return $"**Streaming** {activity.Name}";
			}

			return null;
		}

		private DiscordColor GetHighestColor(DiscordMember member, DiscordColor fallback)
		{
			List<DiscordRole> roles = member.Roles.OrderByDescending((role) => role.Position).ToList();

			DiscordRole highestRoleWithColor = roles.Find((role) => role.Color.Value > 0);
			return highestRoleWithColor?.Color ?? fallback;
		}
	}
}