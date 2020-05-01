namespace Axion.Core.Structures.Interactivity
{
	public class LazyObject<T>
	{
		public readonly bool isCompleted;
		public readonly bool isTimedout;
		public readonly T Result;

		public LazyObject(bool completed, bool timedout, T result = default)
		{
			isCompleted = completed;
			isTimedout = timedout;
			Result = result;
		}

		public static LazyObject<T> FromResult(T result) =>
			new LazyObject<T>(true, false, result);

		public static LazyObject<T> Timedout() =>
			new LazyObject<T>(false, true);
	}
}