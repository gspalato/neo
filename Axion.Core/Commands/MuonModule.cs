using Axion.Core.Utilities;
using Axion.Core.Services;
using Discord;
using Qmmands;
using System.Threading.Tasks;

namespace Axion.Commands
{
	public abstract class AxionModule : ModuleBase<AxionContext>
	{
		public ICommandService CommandService { get; }
		public IMusicService MusicService { get; set; }


		protected EmbedBuilder CreateOkEmbed(string title, string content = null)
		{
			return new EmbedBuilder()
				.WithTitle(title)
				.WithSuccess()
				.WithFooter(new EmbedFooterBuilder
				{
					IconUrl = Context.User.GetAvatarUrl(),
					Text = Context.User.Username
				})
				.WithDescription(content ?? string.Empty);
		}
		protected EmbedBuilder CreateOkEmbed(string content = null) =>
			CreateOkEmbed("", content);

		protected EmbedBuilder CreateErrorEmbed(string title, string content = null)
		{
			return new EmbedBuilder()
				.WithTitle(title)
				.WithError()
				.WithFooter(new EmbedFooterBuilder
				{
					IconUrl = Context.User.GetAvatarUrl(),
					Text = Context.User.Username
				})
				.WithDescription(content ?? string.Empty);
		}
		protected EmbedBuilder CreateErrorEmbed(string content = null) =>
			CreateErrorEmbed("", content);

		protected EmbedBuilder CreateDefaultEmbed(string title, string content = null)
		{
			return new EmbedBuilder()
				.WithTitle(title)
				.WithDefaultColor()
				.WithDescription(content ?? string.Empty);
		}
		protected EmbedBuilder CreateDefaultEmbed(string content = null) =>
			CreateDefaultEmbed("", content);

		protected async Task<IUserMessage> SendOkAsync(string title, string content = null) =>
			await SendEmbedAsync(CreateOkEmbed(title, content));
		protected async Task<IUserMessage> SendOkAsync(string content = null) =>
			await SendOkAsync("", content);

		protected async Task<IUserMessage> SendErrorAsync(string title, string content = null) =>
			await SendEmbedAsync(CreateErrorEmbed(title, content));
		protected async Task<IUserMessage> SendErrorAsync(string content = null) =>
			await SendErrorAsync("", content);

		protected async Task<IUserMessage> SendDefaultEmbedAsync(string title, string content) =>
			await SendEmbedAsync(CreateDefaultEmbed(title, content));
		protected async Task<IUserMessage> SendDefaultEmbedAsync(string description) =>
			await SendDefaultEmbedAsync("", description);

		protected async Task<IUserMessage> SendEmbedAsync(Embed embed) =>
			await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
		protected async Task<IUserMessage> SendEmbedAsync(EmbedBuilder embed) =>
			await Context.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
	}
}