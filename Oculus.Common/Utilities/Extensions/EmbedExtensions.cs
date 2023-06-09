using Discord;

namespace Oculus.Common.Utilities.Extensions
{
    public static class DiscordEmbedBuilderExtensions
    {
        public static EmbedBuilder WithDefaultEmbed(this EmbedBuilder builder, string title = "", string description = "",
            Color? color = null, string footer = "")
        {
            var embed = builder
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(footer);

            if (color is null)
                embed = embed.WithDefaultColor();
            else
                embed = embed.WithColor((Color)color!);

            return embed;
        }

        public static EmbedBuilder WithSuccess(this EmbedBuilder builder)
            => builder.WithColor(new Color(0, 255, 0));

        public static EmbedBuilder WithInfo(this EmbedBuilder builder)
            => builder.WithColor(new Color(0, 40, 153));

        public static EmbedBuilder WithWarning(this EmbedBuilder builder)
            => builder.WithColor(Color.Orange);

        public static EmbedBuilder WithError(this EmbedBuilder builder)
            => builder.WithColor(new Color(255, 0, 0));

        public static EmbedBuilder WithDefaultColor(this EmbedBuilder builder)
            => builder.WithColor(new Color(47, 49, 54));
    }
}