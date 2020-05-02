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

		private TaskCompletionSource<IMessage> _tcs;

		public MessageAwaiter(DiscordSocketClient client,
			ITextChannel channel, Func<IMessage, bool> filter)
		{
			_client = client;

			Channel = channel;
			Filter = filter;

			_tcs = new TaskCompletionSource<IMessage>();
		}

		public Task<IMessage> Wait(int millisecondsTimeout = 180000)
		{
			_client.MessageReceived += HandleMessage;

			_ = Task.Run(async () =>
			{
				var finished = await Task.WhenAny(_tcs.Task, Task.Delay(millisecondsTimeout));
				if (finished != _tcs.Task)
					_tcs.SetCanceled();
			});

			return _tcs.Task;
		}

		private Task HandleMessage(SocketMessage message)
		{
			if (Filter(message))
			{
				_tcs.SetResult(message);
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
