using Discord;
using System.Text;

namespace Neo.Common.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string FromBase64(this string s)
        {
            var base64EncodedBytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string ToBase64(this string s)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Truncate(this string s, int maxLength = 40, bool ellipsis = true)
        {
            return (s.Length > maxLength) ? s.Substring(0, maxLength) + (ellipsis ? "..." : "") : s;
        }

        public static string TruncateAndSanitize(this string s, int maxLength = 40) =>
            Truncate(Format.Sanitize(s), maxLength).Replace("\\.", ".");

        public static string Sanitize(this string s) =>
            Format.Sanitize(s);

        public static string EscapeCodeblock(this string s) =>
            s.Replace("```", $"`\u200B`\u200B`");

        public static string ReplaceAt(this string str, int index, int length, string replace)
        {
            return str.Remove(index, Math.Min(length, str.Length - index))
                    .Insert(index, replace);
        }
    }
}