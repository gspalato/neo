using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Muon.Kernel.Structures
{
	public class Set<T> : IEnumerable<T>
	{
		public T[] array = new T[] { };

		public Set(params T[] array)
		{
			this.array = array;
		}

		public int n
		{
			get { return array.Length; }
		}

		public bool Contains(T value) // Contains value
			=> this.array.Contains(value);

		public bool Contains(Set<T> set) // Contains set
			=> (set % this) == set;

		public static Set<T> operator +(Set<T> first, Set<T> second) // Union
		{
			T[] newSetArray = first.array.Union(second.array).ToArray();
			return new Set<T>(newSetArray);
		}

		public static Set<T> operator -(Set<T> first, Set<T> second) // Difference
		{
			List<T> firstList = new List<T>(first.array);
			List<T> secondList = new List<T>(second.array);

			T[] subtractedArray = firstList.Except(secondList).ToArray();

			return new Set<T>(subtractedArray);
		}

		public static Set<T> operator %(Set<T> first, Set<T> second) // Intersection
		{
			List<T> firstList = new List<T>(first.array);
			List<T> secondList = new List<T>(second.array);

			T[] intersectedArray = firstList.Intersect(secondList).ToArray();

			return new Set<T>(intersectedArray);
		}

		public static bool operator *(Set<T> set, Set<T> comparable) // Contains set
			=> comparable.Contains(set);

		public static bool Equals(Set<T> first, Set<T> second) // Equality
			=> first.array == second.array;

		public static bool Equals(T value, Set<T> set) // Contains
			=> set.Contains(value);

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)array).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)array).GetEnumerator();
		}
	}
}
