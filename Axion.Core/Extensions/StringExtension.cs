using System.Linq;

namespace Axion.Core.Utilities
{
	public static class StringExtensions
	{
		public static string Truncate(this string s, int maxLength = 40)
		{
			return s != null && s.Length > maxLength ? s.Substring(0, maxLength) + "..." : s;
		}

		public static string Escape(this string s, char[] chars = null, bool useWhitespace = false)
		{
			chars ??= new char[] { '*', '_', '~', '|', '`' };

			string escaped = "";
			foreach (char character in s)
			{
				if (chars.Any(c => c == character))
					escaped += (useWhitespace ? "\u200B" : "\\") + character;
				else
					escaped += character;
			}

			return escaped;
		}
		public static string Escape(this string s, char c, bool useWhitespace = false) =>
			s.Escape(new char[] { c }, useWhitespace);

		public static string TruncateAndEscape(this string s, int maxLength = 40)
		{
			return Escape(Truncate(s, maxLength));
		}
	}
}