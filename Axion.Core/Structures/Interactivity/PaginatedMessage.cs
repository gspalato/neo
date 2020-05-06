using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	using ReactionBehavior = ValueTuple<Emoji, Action<PaginatedContext>>;

    public interface IPaginatedMessage : IDisposable
	{
		List<ReactionBehavior> Callbacks { get; }
		int CurrentPage { get; }
		IUser Responsible { get; }
		IUserMessage Message { get; }
		Embed[] Pages { get; }

		new void Dispose();
		Task SkipToPageAsync(int page);
		Task<IUserMessage> Send(ITextChannel channel);
	}

	public class PaginatedMessage : IPaginatedMessage
	{
		protected bool IsDisposed;
		private readonly object _lock = new object();

		private readonly DiscordSocketClient _client;

		public List<ReactionBehavior> Callbacks { get; }
		public int CurrentPage { get; private set; } = 0;
		public IUser Responsible { get; }
		public IUserMessage Message { get; private set; }
		public Embed[] Pages { get; }

		private readonly int _millisecondsTimeout;

		public PaginatedMessage(DiscordSocketClient client, IUser responsible,
			Embed[] pages, List<ReactionBehavior> callbacks, int millisecondsTimeout = 180000)
		{
			_client = client;

			Callbacks = callbacks;
			Pages = pages;
			Responsible = responsible;

			_millisecondsTimeout = millisecondsTimeout;
		}

		public Task SkipToPageAsync(int page)
		{
			lock (_lock)
			{
				CurrentPage = page;

				return Message.ModifyAsync(props =>
				{
					props.Embed = Pages[CurrentPage];
				});
			}
		}

		public async Task<IUserMessage> Send(ITextChannel channel)
		{
			Message = await channel.SendMessageAsync(embed: Pages[0]);

			await AddButtons();
			_client.ReactionAdded += HandleReaction;

			_ = Task.Run(async () =>
			{
				await Task.Delay(_millisecondsTimeout);
				await Message.RemoveAllReactionsAsync();
				Dispose();
			});

			return Message;
		}

		private Task AddButtons()
		{
			return Message.AddReactionsAsync(Callbacks.Select(t => t.Item1 as IEmote).ToArray());
		}

		private async Task HandleReaction(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!(channel is ITextChannel textChannel))
				return;

			var me = await textChannel.Guild.GetCurrentUserAsync();
			if (reaction.UserId == me.Id)
				return;

			if (reaction.MessageId != Message.Id)
				return;

			if (Responsible.Id != reaction.UserId)
				return;

			var user = reaction.User.IsSpecified
				? reaction.User.Value
				: await channel.GetUserAsync(reaction.UserId);

			if (!me.GetPermissions(textChannel).ManageMessages)
				throw new Exception("I lack permissions to manage messages.");

			if (!Callbacks.Select(t => t.Item1.Name).Contains(reaction.Emote.Name))
				return;


			var ctx = new PaginatedContext
			{
				PaginatedMessage = this,
				User = user,
				Reaction = reaction
			};

			var callback = Callbacks.First(t => t.Item1.Name == reaction.Emote.Name).Item2;
			callback.Invoke(ctx);
		}

		public void Dispose()
		{
			_client.ReactionAdded -= HandleReaction;
			IsDisposed = true;
		}

		~PaginatedMessage()
		{
			Dispose();
		}
	}
}
