using System.Collections.Generic;
using System.Linq;

namespace Axion.Kernel.Utilities
{
	public static class StringExtensions
	{
		public static string Truncate(this string s, int maxLength = 40)
		{
			return s != null && s.Length > maxLength ? s.Substring(0, maxLength) + "..." : s;
		}

		public static string Escape(this string s, char[] chars = null)
		{
			chars ??= new char[] { '*', '_', '~', '|', '`' };

			string escaped = "";
			foreach (char character in s)
			{
				if (chars.Any((char c) => c == character))
					escaped += "\\" + character;
				else
					escaped += character;
			}

			return escaped;
		}
		public static string Escape(this string s, char c) =>
			s.Escape(new char[] { c });

		public static string TruncateAndEscape(this string s, int maxLength = 40)
		{
			return Escape(Truncate(s, maxLength));
		}
	}
}