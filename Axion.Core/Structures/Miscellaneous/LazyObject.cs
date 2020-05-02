using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public class LazyObject<T>
	{
		public readonly TaskCompletionSource<T> TaskSource = new TaskCompletionSource<T>();

		public bool IsCompleted => Result.IsCompleted;
		public bool IsTimedout { get; private set; }
		public Task<T> Result => TaskSource.Task;

		public LazyObject() { }

		public LazyObject(bool isTimedout, T result = default)
		{
			IsTimedout = isTimedout;

			if (!(result is null))
				Finish(result);
		}

		public void Finish(T result)
		{
			IsTimedout = false;
			TaskSource.SetResult(result);
		}

		public void Timeout()
		{
			IsTimedout = true;
			TaskSource.SetResult(default);
		}
	}
}