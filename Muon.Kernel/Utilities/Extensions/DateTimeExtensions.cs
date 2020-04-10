using System;
using System.Collections.Generic;
using System.Text;

namespace Muon.Kernel.Utilities
{
    public static class DateTimeExtensions
    {
        public static string ToHumanDuration(this TimeSpan duration, bool displaySign = false)
        {
            if (duration == null)
                return null;

            var builder = new StringBuilder();
            if (displaySign)
                builder.Append(duration.TotalMilliseconds < 0 ? "-" : "+");

            duration = duration.Duration();

            if (duration.Days > 0)
                builder.Append($"{duration.Days}d ");

            if (duration.Hours > 0)
                builder.Append($"{duration.Hours}h ");

            if (duration.Minutes > 0)
                builder.Append($"{duration.Minutes}m ");

            if (duration.TotalHours < 1)
                if (duration.Seconds > 0)
                {
                    builder.Append(duration.Seconds);
                    builder.Append("s ");
                }
                else
                    if (duration.Milliseconds > 0)
                        builder.Append($"{duration.Milliseconds}ms ");

            if (builder.Length <= 1)
                builder.Append(" <1ms ");

            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }
    }
}
