using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Arpa.Utilities
{
	public static class StringExtensions
	{
		public static string Truncate(this string s, int maxLength)
		{
			return s != null && s.Length > maxLength ? s.Substring(0, maxLength) + "..." : s;
		}

		public static string Escape(this string s)
		{
			List<char> escapeSequences = new List<char>
			{
				'*',
				'_',
				'~',
				'|'
			};

			string escaped = "";
			foreach (char character in s)
			{
				if (escapeSequences.Any((char c) => c == character))
				{
					escaped += "\\";
					escaped += character;
				}
				else
				{
					escaped += character;
				}
			}

			return escaped;
		}

		public static string TruncateAndEscape(this string s, int maxLength)
		{
			return Escape(Truncate(s, maxLength));
		}
	}

	public static class LinqExtensions
	{
		public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> fullBatch, int chunkSize)
		{
			if (chunkSize <= 0)
			{
				throw new ArgumentOutOfRangeException(
					"chunkSize",
					chunkSize,
					"Chunk size cannot be less than or equal to zero.");
			}

			if (fullBatch == null)
			{
				throw new ArgumentNullException("fullBatch", "Input to be split cannot be null.");
			}

			var cellCounter = 0;
			var chunk = new List<T>(chunkSize);

			foreach (var element in fullBatch)
			{
				if (cellCounter++ == chunkSize)
				{
					yield return chunk;
					chunk = new List<T>(chunkSize);
					cellCounter = 1;
				}

				chunk.Add(element);
			}

			yield return chunk;
		}
	}
}