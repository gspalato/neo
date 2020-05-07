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
			ITextChannel channel, Predicate<IMessage> filter) : base(client, filter)
		{
			Channel = channel;
		}

		public override Task<IMessage> Wait(int millisecondsTimeout = 180000)
		{
			Client.MessageReceived += HandleEvent;

			return base.Wait(millisecondsTimeout);
		}

		private Task HandleEvent(SocketMessage message)
		{
			if (Filter(message))
			{
				Tcs.SetResult(message);
				Dispose();
			}

			return Task.CompletedTask;
		}

		public override void Dispose()
		{
			Client.MessageReceived -= HandleEvent;
			base.Dispose();
		}

		~MessageAwaiter()
		{
			Dispose();
		}
	}
}
