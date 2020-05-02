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

		private TaskCompletionSource<SocketReaction> _tcs;

		private bool _shouldDeleteReaction;

		public ReactionAwaiter(DiscordSocketClient client,
			IUserMessage message, Func<SocketReaction, bool> filter,
			bool shouldDeleteReaction = true)
		{
			_client = client;

			Message = message;
			Filter = filter;

			_tcs = new TaskCompletionSource<SocketReaction>();

			_shouldDeleteReaction = shouldDeleteReaction;
		}

		public Task<SocketReaction> Wait(int millisecondsTimeout = 180000)
		{
			_client.ReactionAdded += HandleReaction;

			_ = Task.Run(async () =>
			{
				var finished = await Task.WhenAny(_tcs.Task, Task.Delay(millisecondsTimeout));
				if (finished != _tcs.Task)
					_tcs.SetCanceled();
			});

			return _tcs.Task;
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
				_tcs.SetResult(reaction);
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
