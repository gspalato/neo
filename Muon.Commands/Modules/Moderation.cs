using Discord;
using Discord.WebSocket;
using Muon.Kernel.Structures;
using Muon.Kernel.Utilities;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Muon.Commands
{
	[Category("Moderation")]
	[Description("Le ban hammer")]
	public sealed class Moderation : MuonModule
	{
		[Command("whois")]
		public async Task WhoIsAsync(SocketGuildUser member)
		{
			try
			{
				string escapedUsername = member.Username.Escape();
				string escapedNickname = member.Nickname?.Escape();

				string totalName = $"{escapedUsername} {(escapedNickname is null ? "" : $"({escapedNickname})")}";

				string status = string.IsNullOrEmpty(member.GetStatus())
					? ""
					: $"> {member.GetStatus()}";

				Console.WriteLine(status);

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
			catch (Exception e)
			{
				Console.WriteLine($"{e.Message}\n{e.StackTrace}");
			}
		}
	}
}