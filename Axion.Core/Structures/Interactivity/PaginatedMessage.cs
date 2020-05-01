using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public class PaginatedMessage : IDisposable
	{
		protected bool isDisposed = false;

		private readonly DiscordSocketClient _client;

		protected IUserMessage Message;
		public readonly IUser Author;
		public readonly Embed[] Pages;

		private int currentPage = 1;

		public PaginatedMessage(DiscordSocketClient client, IUser author, Embed[] pages)
		{
			_client = client;

			Author = author;
			Pages = pages;
		}

		public async Task<IUserMessage> Send(ITextChannel channel)
		{
			Message = await channel.SendMessageAsync(embed: Pages[0]);

			_ = AddButtons();
			_client.ReactionAdded += HandleReaction;

			return Message;
		}

		private async Task AddButtons()
		{
			await Message.AddReactionAsync(new Emoji("⏪"));
			await Message.AddReactionAsync(new Emoji("◀️"));
			await Message.AddReactionAsync(new Emoji("⏹️"));
			await Message.AddReactionAsync(new Emoji("▶️"));
			await Message.AddReactionAsync(new Emoji("⏩"));
		}

		private async Task HandleReaction(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!(channel is ITextChannel textChannel))
				return;

			var message = cache.HasValue
				? cache.Value
				: reaction.Message.IsSpecified
					? reaction.Message.Value
					: await channel.GetMessageAsync(reaction.MessageId);

			var me = await textChannel.Guild.GetCurrentUserAsync();
			if (reaction.UserId == me.Id)
				return;

			if (reaction.MessageId != Message.Id)
				return;

			if (reaction.UserId != Author.Id)
				return;

				if (!me.GetPermissions(textChannel).ManageMessages)
				throw new Exception("I lack permissions to manage messages.");

			switch (reaction.Emote.Name)
			{
				case "⏪":
					{
						currentPage = 0;
						await Message.ModifyAsync(props =>
						{
							props.Embed = Pages[0];
						});

						await Message.RemoveReactionAsync(reaction.Emote, Author);
					}
					break;

				case "◀️":
					{
						if (currentPage <= 0)
						{
							await Message.RemoveReactionAsync(reaction.Emote, Author);
							return;
						}

						--currentPage;
						await Message.ModifyAsync(props =>
						{
							props.Embed = Pages[currentPage];
						});

						await Message.RemoveReactionAsync(reaction.Emote, Author);
					}
					break;

				case "⏹️":
					{
						await Message.DeleteAsync();
						Dispose();
					}
					break;

				case "▶️":
					{
						if (currentPage >= Pages.Length - 1)
						{
							await Message.RemoveReactionAsync(reaction.Emote, Author);
							return;
						}

						++currentPage;
						await Message.ModifyAsync(props =>
						{
							props.Embed = Pages[currentPage];
						});

						await Message.RemoveReactionAsync(reaction.Emote, Author);
					}
					break;

				case "⏩":
					{
						if (currentPage == Pages.Length - 1)
							return;

						currentPage = Pages.Length - 1;
						await Message.ModifyAsync(props =>
						{
							props.Embed = Pages[currentPage];
						});

						await Message.RemoveReactionAsync(reaction.Emote, Author);
					}
					break;
			}
		}

		public void Dispose()
		{
			_client.ReactionAdded -= HandleReaction;
			isDisposed = true;
		}
	}
}
