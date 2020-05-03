using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public interface IEventAwaiter<T> : IDisposable
	{
		Task<T> Wait(int millisecondsTimeout = 180000);
		new void Dispose();
	}

	public class EventAwaiter<T> : IEventAwaiter<T>
	{
		public Func<T, bool> Filter { get; private set; }

		protected bool _isDisposed = false;
		protected readonly DiscordSocketClient _client;
		protected TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>();

		public EventAwaiter(DiscordSocketClient client, Func<T, bool> filter)
		{
			_client = client;

			Filter = filter;
		}

		public virtual Task<T> Wait(int millisecondsTimeout = 180000)
		{
			_ = Task.Run(async () =>
			{
				var finished = await Task.WhenAny(_tcs.Task, Task.Delay(millisecondsTimeout));
				if (finished != _tcs.Task)
					_tcs.SetCanceled();
			});

			return _tcs.Task;
		}

		public virtual void Dispose()
		{
			_isDisposed = true;
		}
	}
}
