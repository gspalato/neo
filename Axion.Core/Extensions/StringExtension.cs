using Discord;
using System;

namespace Axion.Core.Extensions
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

		public static string EscapeCodeblock(this string s) =>
			s.Replace("```", $"`\u200B`\u200B`");

		public static string ReplaceAt(this string str, int index, int length, string replace)
		{
			return str.Remove(index, Math.Min(length, str.Length - index))
					.Insert(index, replace);
		}
	}
}