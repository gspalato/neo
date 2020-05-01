namespace Axion.Core.Structures.Interactivity
{
    public class LazyObject<T>
    {
        public readonly bool isCompleted;
        public readonly bool isTimedout;
        public readonly T Result;

        public LazyObject(bool completed, bool timedout, T result)
        {
            isCompleted = completed;
            isTimedout = timedout;
            Result = result;
        }
    }
}
