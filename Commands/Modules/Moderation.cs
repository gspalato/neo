using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Muon.Kernel.Structures.Attributes;
using Muon.Kernel.Utilities;
using Qmmands;

namespace Muon.Commands.Modules
{
	[Category("Moderation")]
	[Description("Le ban hammer")]
	public sealed class Moderation : MuonModule
	{
		/*
		private InteractivityService Interactivity { get; }

		public Moderation(IServiceProvider services)
		{
			Interactivity = services.GetRequiredService<InteractivityService>();
		}

		[Command("ban")]
		[RequireGuildBotPermissions(GuildPermission.BanMembers)]
		[RequireGuildUserPermissions(GuildPermission.BanMembers)]
		public async Task BanAsync(IGuildUser member, string reason = "")
		{
			var embed = new EmbedBuilder()
				.WithTitle("Confirmation")
				.WithDescription($"Are you sure you want to ban {member.Mention} for `{reason.TruncateAndEscape()}`")
				.WithDefaultColor();

			var abortedEmbed = new EmbedBuilder()
				.WithTitle("Aborted")
				.WithDescription($"Aborted ban. You'll get away this time.")
				.WithWarning();

			var request = new ConfirmationBuilder()
				.WithContent(embed: embed)
				.WithCancelledEmbed(abortedEmbed)
				.WithTimeoutedEmbed(abortedEmbed)
				.WithUsers(Context.User as SocketUser)
				.Build();

			var result = await Interactivity.SendConfirmationAsync(request, Context.Channel);

			if (result.IsSuccess)
			{
				try
				{
					await member.BanAsync(reason: reason);

					var successEmbed = new EmbedBuilder()
						.WithTitle("Success")
						.WithDescription($"Banned {member.Mention} for `{reason.TruncateAndEscape()}`")
						.WithSuccess();

					await SendEmbedAsync(successEmbed);
				}
				catch
				{
					var errorEmbed = new EmbedBuilder()
						.WithTitle("Error")
						.WithDescription($"Couldn't ban {member.Mention}. Check if I have enough permissions.")
						.WithError();

					await SendEmbedAsync(errorEmbed);
				}
			}
		}
		*/

		[Command("purge")]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[RequireChannelUserPermissions(ChannelPermission.ManageMessages)]
		[IgnoresExtraArguments]
		public async Task PurgeAsync(int number)
		{
			var channel = Context.Channel;
			var messages = await Context.Channel.GetMessagesAsync(number).ElementAtAsync(1);

			await channel.DeleteMessagesAsync(messages);

			var ok = await SendOkAsync($"Purged {messages.Count().ToString()} messages.");
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