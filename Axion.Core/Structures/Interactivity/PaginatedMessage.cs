using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	using ReactionBehavior = ValueTuple<Emoji, Action<PaginatedContext>>;

	public class PaginatedMessage : IDisposable
	{
		private bool _isDisposed = false;
		private object _lock = new object();

		private readonly DiscordSocketClient _client;

		public List<ReactionBehavior> Callbacks { get; private set; }
		public int CurrentPage { get; private set; } = 0;
		public IList<IUser> Responsibles { get; private set; }
		public IUserMessage Message { get; private set; }
		public Embed[] Pages { get; private set; }

		private int _millisecondsTimeout;

		public PaginatedMessage(DiscordSocketClient client, IList<IUser> responsibles,
			Embed[] pages, List<ReactionBehavior> callbacks, int millisecondsTimeout = 180000)
		{
			_client = client;

			Callbacks = callbacks;
			Pages = pages;
			Responsibles = responsibles;

			_millisecondsTimeout = millisecondsTimeout;
		}

		public async Task SkipToPageAsync(int page)
		{
			lock (_lock)
			{
				CurrentPage = page;
			}

			await Message.ModifyAsync(props =>
			{
				props.Embed = Pages[CurrentPage];
			});
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
			_ = Message.AddReactionsAsync(Callbacks.Select(t => t.Item1).ToArray());
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

			var user = reaction.User.IsSpecified
				? reaction.User.Value
				: await channel.GetUserAsync(reaction.UserId);

			var me = await textChannel.Guild.GetCurrentUserAsync();
			if (reaction.UserId == me.Id)
				return;

			if (reaction.MessageId != Message.Id)
				return;

			if (Responsibles.Select(r => r.Id).Contains(reaction.UserId))
				return;

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

			Callbacks.First(t => t.Item1.Name == reaction.Emote.Name).Item2.Invoke(ctx);
		}

		public void Dispose()
		{
			_client.ReactionAdded -= HandleReaction;
			_isDisposed = true;
		}

		~PaginatedMessage()
		{
			Dispose();
		}
	}
}
