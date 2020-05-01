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

		private LazyObject<IMessage> _lazyResult;

		public MessageAwaiter(DiscordSocketClient client,
			ITextChannel channel, Func<IMessage, bool> filter)
		{
			_client = client;

			Channel = channel;
			Filter = filter;
		}

		public LazyObject<IMessage> Wait(int millisecondsTimeout = 180000)
		{
			_client.MessageReceived += HandleMessage;

			_lazyResult = new LazyObject<IMessage>();

			_ = Task.Run(async () =>
			{
				var finished = await Task.WhenAny(_lazyResult.Result, Task.Delay(millisecondsTimeout));
				if (finished != _lazyResult.Result)
					_lazyResult.Timeout();
			});

			return _lazyResult;
		}

		private Task HandleMessage(SocketMessage message)
		{
			if (Filter(message))
			{
				_lazyResult.Finish(message);
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
