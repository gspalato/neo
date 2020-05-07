using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public interface IEventAwaiter<T> : IDisposable
	{
		Predicate<T> Filter { get; }

		Task<T> Wait(int millisecondsTimeout = 180000);
		new void Dispose();
	}

	public abstract class EventAwaiter<T> : IEventAwaiter<T>
	{
		public Predicate<T> Filter { get; }

		protected bool IsDisposed;
		protected readonly DiscordSocketClient Client;
		protected readonly TaskCompletionSource<T> Tcs = new TaskCompletionSource<T>();

		protected EventAwaiter(DiscordSocketClient client, Predicate<T> filter)
		{
			Client = client;

			Filter = filter;
		}

		public virtual Task<T> Wait(int millisecondsTimeout = 180000)
		{
			_ = Task.Run(async () =>
			{
				var finished = await Task.WhenAny(Tcs.Task, Task.Delay(millisecondsTimeout));
				if (finished != Tcs.Task)
				{
					Tcs.SetCanceled();
					Dispose();
				}
			});

			return Tcs.Task;
		}

		public virtual void Dispose()
		{
			IsDisposed = true;
		}
	}
}