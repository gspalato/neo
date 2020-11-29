using Oculus.Common.Extensions;
using Oculus.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System.Threading.Tasks;

namespace Oculus.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Description("Quote someone's message.")]
	[Group("quote", "q")]
	public class Quote : OculusModule
	{
		[Command]
		public async Task QuoteAsync(IMessage message)
		{
			var embedAuthor = new EmbedAuthorBuilder()
				.WithName(message.Author.Username)
				.WithIconUrl(message.Author.GetAvatarUrl());

			var embed = new EmbedBuilder()
				.WithAuthor(embedAuthor)
				.WithDefaultColor()
				.WithDescription($"{message.Content}\n\n[Jump To]({message.GetJumpUrl()})")
				.WithTimestamp(message.Timestamp);

			await SendEmbedAsync(embed);
		}
	}
}
