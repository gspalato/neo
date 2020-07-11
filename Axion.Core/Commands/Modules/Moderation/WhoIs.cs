using Axion.Common.Extensions;
using Axion.Core.Structures.Attributes;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Moderation
{
	[Category(Category.Moderation)]
	[Group("whois")]
	public class WhoIs : AxionModule
	{
		[Command]
		public async Task ExecuteAsync(IGuildUser target)
		{
			if (!(target is SocketGuildUser member))
				return;

			var escapedUsername = Format.Sanitize(member.Username);
			var escapedNickname = member.Nickname != null
				? $"({Format.Sanitize(member.Nickname)})"
				: "";

			var totalName = $"{escapedUsername} {escapedNickname}";

			var rolesList = from role in member.Roles
							orderby role.Position descending
							select Format.Code(Format.Sanitize(role.Name));

			var embed = new EmbedBuilder()
				.WithAuthor($"{member.Username}#{member.Discriminator}", member.GetAvatarUrl())
				.WithDescription(member.GetStatus())
				.WithColor(member.Id == Context.Client.CurrentUser.Id
					? new Color(0x2A8EF4)
					: member.GetHighestColor(new Color()))
				.AddField("Name", totalName, true)
				.AddField("Discriminator", $"#{member.Discriminator}", true)
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
