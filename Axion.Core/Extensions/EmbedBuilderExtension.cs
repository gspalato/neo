using Discord;

namespace Axion.Core.Utilities
{
    public static class DiscordEmbedBuilderExtensions
    {
        public static EmbedBuilder WithSuccess(this EmbedBuilder builder)
            => builder.WithColor(Color.Green);

        public static EmbedBuilder WithInfo(this EmbedBuilder builder)
            => builder.WithColor(new Color(42, 142, 244));

        public static EmbedBuilder WithWarning(this EmbedBuilder builder)
            => builder.WithColor(Color.Orange);

        public static EmbedBuilder WithError(this EmbedBuilder builder)
            => builder.WithColor(Color.Red);

        public static EmbedBuilder WithDefaultColor(this EmbedBuilder builder)
            => builder.WithColor(new Color(47, 49, 54));
    }
}
