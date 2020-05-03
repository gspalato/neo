using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public class MessageAwaiter : EventAwaiter<IMessage>, IEventAwaiter<IMessage>
	{
		public readonly ITextChannel Channel;

		public MessageAwaiter(DiscordSocketClient client,
			ITextChannel channel, Func<IMessage, bool> filter) : base(client, filter)
		{
			Channel = channel;
		}

		public override Task<IMessage> Wait(int millisecondsTimeout = 180000)
		{
			_client.MessageReceived += HandleEvent;

			return base.Wait(millisecondsTimeout);
		}

		private Task HandleEvent(SocketMessage message)
		{
			if (Filter(message))
			{
				_tcs.SetResult(message);
				Dispose();
			}

			return Task.CompletedTask;
		}

		public override void Dispose()
		{
			_client.MessageReceived -= HandleEvent;
			base.Dispose();
		}

		~MessageAwaiter()
		{
			Dispose();
		}
	}
}
