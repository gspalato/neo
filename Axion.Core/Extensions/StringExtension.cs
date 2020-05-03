using Discord;

namespace Axion.Core.Utilities
{
	public static class StringExtensions
	{
		public static string Truncate(this string s, int maxLength = 40)
		{
			return s != null && s.Length > maxLength ? s.Substring(0, maxLength) + "..." : s;
		}

		public static string TruncateAndSanitize(this string s, int maxLength = 40)
		{
			return Format.Sanitize(Truncate(s, maxLength));
		}
	}
}