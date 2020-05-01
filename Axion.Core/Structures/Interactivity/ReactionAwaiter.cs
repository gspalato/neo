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

		private TaskCompletionSource<SocketReaction> _task = new TaskCompletionSource<SocketReaction>();

		public ReactionAwaiter(DiscordSocketClient client,
			IUserMessage message, Func<SocketReaction, bool> filter)
		{
			_client = client;

			Message = message;
			Filter = filter;
		}

		public async Task<LazyObject<SocketReaction>> Run(int millisecondsTimeout = 180000)
		{
			_client.ReactionAdded += HandleReaction;
			var reactionTask = _task.Task;
			var finishedFirst = await Task.WhenAny(reactionTask, Task.Delay(millisecondsTimeout));

			if (finishedFirst == reactionTask)
				return new LazyObject<SocketReaction>(true, false, reactionTask.Result);
			else
				return new LazyObject<SocketReaction>(false, true, null);
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
				_task.SetResult(reaction);
				Dispose();
			}
		}

		public void Dispose()
		{
			_client.ReactionAdded -= HandleReaction;
			isDisposed = true;
		}
	}
}
