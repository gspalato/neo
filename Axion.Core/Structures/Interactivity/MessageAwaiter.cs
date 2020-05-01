using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public class MessageAwaiter : IDisposable
	{
		protected bool isDisposed = false;

		private readonly DiscordSocketClient _client;

		public readonly ITextChannel Channel;
		public readonly Func<IMessage, bool> Filter;

		private TaskCompletionSource<IMessage> _task = new TaskCompletionSource<IMessage>();

		public MessageAwaiter(DiscordSocketClient client,
			ITextChannel channel, Func<IMessage, bool> filter)
		{
			_client = client;

			Channel = channel;
			Filter = filter;
		}

		public async Task<LazyObject<IMessage>> Wait(int millisecondsTimeout = 180000)
		{
			_client.MessageReceived += HandleMessage;

			var messageTask = _task.Task;
			var finishedFirst = await Task.WhenAny(messageTask, Task.Delay(millisecondsTimeout));

			if (finishedFirst == messageTask)
				return LazyObject<IMessage>.FromResult(messageTask.Result);
			else
				return LazyObject<IMessage>.Timedout();
		}

		private Task HandleMessage(SocketMessage message)
		{
			if (Filter(message))
			{
				_task.SetResult(message);
				Dispose();
			}

			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_client.MessageReceived -= HandleMessage;
			isDisposed = true;
		}

		~MessageAwaiter()
		{
			Dispose();
		}
	}
}
