using DSharpPlus.Entities;

namespace Muon.Kernel.Utilities
{
    public static class DiscordEmbedBuilderExtensions
    {
        public static DiscordEmbedBuilder WithSuccess(this DiscordEmbedBuilder builder)
            => builder.WithColor(new DiscordColor(0x00FF00));

        public static DiscordEmbedBuilder WithInfo(this DiscordEmbedBuilder builder)
            => builder.WithColor(new DiscordColor(0x2A8EF4));

        public static DiscordEmbedBuilder WithWarning(this DiscordEmbedBuilder builder)
            => builder.WithColor(new DiscordColor(0x2F3136));

        public static DiscordEmbedBuilder WithError(this DiscordEmbedBuilder builder)
            => builder.WithColor(new DiscordColor(0xFF0000));

        public static DiscordEmbedBuilder WithDefaultColor(this DiscordEmbedBuilder builder)
            => builder.WithColor(new DiscordColor(0x2F3136));
    }
}
