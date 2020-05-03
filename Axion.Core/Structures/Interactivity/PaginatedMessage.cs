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

		public IUser Author { get; private set; }
		public IUserMessage Message { get; private set; }
		public Embed[] Pages { get; private set; }

		public int CurrentPage { get; private set; } = 0;
		private int _millisecondsTimeout;

		public PaginatedMessage(DiscordSocketClient client, IUser author, Embed[] pages, int millisecondsTimeout = 180000)
		{
			_client = client;

			Author = author;
			Pages = pages;

			_millisecondsTimeout = millisecondsTimeout;
		}

		public async Task<IUserMessage> Send(ITextChannel channel)
		{
			Message = await channel.SendMessageAsync(embed: Pages[0]);

			AddButtons();
			_client.ReactionAdded += HandleReaction;

			_ = Task.Run(async () =>
			{
				await Task.Delay(_millisecondsTimeout);
				await Message.RemoveAllReactionsAsync();
				Dispose();
			});

			return Message;
		}

		private void AddButtons()
		{
			_ = Message.AddReactionsAsync(new[]
			{
				new Emoji("⏪"),
				new Emoji("◀️"),
				new Emoji("⏹️"),
				new Emoji("▶️"),
				new Emoji("⏩")
			});
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
						CurrentPage = 0;
						await Message.ModifyAsync(props =>
						{
							props.Embed = Pages[0];
						});

						await Message.RemoveReactionAsync(reaction.Emote, Author);
					}
					break;

				case "◀️":
					{
						if (CurrentPage <= 0)
						{
							await Message.RemoveReactionAsync(reaction.Emote, Author);
							return;
						}

						--CurrentPage;
						await Message.ModifyAsync(props =>
						{
							props.Embed = Pages[CurrentPage];
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
						if (CurrentPage >= Pages.Length - 1)
						{
							await Message.RemoveReactionAsync(reaction.Emote, Author);
							return;
						}

						++CurrentPage;
						await Message.ModifyAsync(props =>
						{
							props.Embed = Pages[CurrentPage];
						});

						await Message.RemoveReactionAsync(reaction.Emote, Author);
					}
					break;

				case "⏩":
					{
						if (CurrentPage == Pages.Length - 1)
							return;

						CurrentPage = Pages.Length - 1;
						await Message.ModifyAsync(props =>
						{
							props.Embed = Pages[CurrentPage];
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

		~PaginatedMessage()
		{
			Dispose();
		}
	}
}
