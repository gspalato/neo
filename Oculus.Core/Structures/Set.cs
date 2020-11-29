using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oculus.Core.Structures
{
	public class Set<T> : IEnumerable<T>
	{
		public T[] Array;

		public Set(params T[] array)
		{
			Array = array;
		}

		public int N
		{
			get => Array.Length;
		}

		public bool Contains(T value) // Contains value
			=> Array.Contains(value);

		public bool Contains(Set<T> set) // Contains set
			=> (set % this) == set;

		public static Set<T> operator +(Set<T> first, Set<T> second) // Union
		{
			var newSetArray = first.Array.Union(second.Array).ToArray();
			return new Set<T>(newSetArray);
		}

		public static Set<T> operator -(Set<T> first, Set<T> second) // Difference
		{
			var firstList = first.Array.ToList();
			var secondList = second.Array.ToList();

			var subtractedArray = firstList.Except(secondList).ToArray();

			return new Set<T>(subtractedArray);
		}

		public static Set<T> operator %(Set<T> first, Set<T> second) // Intersection
		{
			var firstList = first.Array.ToList();
			var secondList = second.Array.ToList();

			var intersectedArray = firstList.Intersect(secondList).ToArray();

			return new Set<T>(intersectedArray);
		}

		public static bool operator *(Set<T> set, Set<T> comparable) // Contains set
			=> comparable.Contains(set);

		public static bool Equals(Set<T> first, Set<T> second) // Equality
			=> first.Array == second.Array;

		public static bool Equals(T value, Set<T> set) // Contains
			=> set.Contains(value);

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)Array).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)Array).GetEnumerator();
		}
	}
}
