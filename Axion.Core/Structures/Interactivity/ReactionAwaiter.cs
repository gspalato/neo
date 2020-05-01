using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public class ReactionAwaiter : IDisposable
	{
		protected bool isDisposed = false;

		private readonly DiscordSocketClient _client;

		public readonly IUserMessage Message;
		public readonly Func<SocketReaction, bool> Filter;

		private LazyObject<SocketReaction> _lazyResult;

		private bool _shouldDeleteReaction;

		public ReactionAwaiter(DiscordSocketClient client,
			IUserMessage message, Func<SocketReaction, bool> filter,
			bool shouldDeleteReaction = true)
		{
			_client = client;

			Message = message;
			Filter = filter;

			_shouldDeleteReaction = shouldDeleteReaction;
		}

		public LazyObject<SocketReaction> Wait(int millisecondsTimeout = 10000)
		{
			_client.ReactionAdded += HandleReaction;

			_lazyResult = new LazyObject<SocketReaction>();

			_ = Task.Run(async () =>
			{
				var finished = await Task.WhenAny(_lazyResult.Result, Task.Delay(millisecondsTimeout));
				if (finished != _lazyResult.Result)
					_lazyResult.Timeout();
			});

			return _lazyResult;
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

			if (Filter(reaction))
			{
				_lazyResult.Finish(reaction);
				if (_shouldDeleteReaction)
					await Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

				Dispose();
			}
		}

		public void Dispose()
		{
			_client.ReactionAdded -= HandleReaction;
			isDisposed = true;
		}

		~ReactionAwaiter()
		{
			Dispose();
		}
	}
}
