using Oculus.Common.Extensions;
using Oculus.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Moderation
{
	[Category(Category.Moderation)]
	[Group("kick")]
	public class Kick : OculusModule
	{
		[Command]
		[RequireChannelBotPermissions(ChannelPermission.ManageMessages)]
		[RequireGuildBotPermissions(GuildPermission.KickMembers)]
		[RequireGuildUserPermissions(GuildPermission.KickMembers)]
		public async Task ExecuteAsync(IGuildUser member, [Remainder] string reason = "Unspecified reason.")
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
	}
}
