using Oculus.Common.Extensions;
using Oculus.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Moderation
{
	[Category(Category.Moderation)]
	[Group("softban")]
	public class Softban : OculusModule
	{
		[Command]
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
	}
}
