using Discord;
using System;

namespace Oculus.Common.Extensions
{
	public static class StringExtensions
	{
		public static string FromBase64(this string s)
		{
			var base64EncodedBytes = Convert.FromBase64String(s);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}
		
		public static string ToBase64(this string s)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(s);
			return Convert.ToBase64String(plainTextBytes);
		}
		
		public static string Truncate(this string s, int maxLength = 40)
		{
			return (s is not null && s.Length > maxLength) ? s.Substring(0, maxLength) + "..." : s;
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