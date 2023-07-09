using Discord;
using Neo.Common.Utilities.Extensions;

namespace Neo.Common.Utilities
{
    public static class Utilities
    {
        public static EmbedBuilder CreateDefaultEmbed(string title = "", string description = "",
            Color? color = null, string footer = "")
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(footer);

            if (color is null)
                embed = embed.WithDefaultColor();
            else
                embed = embed.WithColor((Color)color!);

            return embed;
        }

        public static string ObjectToString(object o)
        {
            if (o == null)
                return "null";

            switch (o)
            {
                case DateTime dt:
                    return dt.ToString("dd-MM-yyyy HH:mm:ss zzz");

                case DateTimeOffset dto:
                    return dto.ToString("dd-MM-yyyy HH:mm:ss zzz");

                case TimeSpan ts:
                    return ts.ToString("c");

                case Enum e:
                    var flags = Enum.GetValues(e.GetType())
                        .OfType<Enum>()
                        .Where(xev => e.HasFlag(xev))
                        .Select(xev => xev.ToString());
                    return string.Concat(", ", flags);

                default:
                    return o.ToString() ?? "";
            }
        }
    }
}
