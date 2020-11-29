using Oculus.Common.Extensions;
using Discord;
using Qmmands;
using System.Threading.Tasks;

namespace Oculus.Core.Commands
{
	public abstract class OculusModule : ModuleBase<OculusContext>
	{
		public ICommandService CommandService => Context.GetService<ICommandService>();

		protected EmbedBuilder CreateOkEmbed(string title, string content)
		{
			return new EmbedBuilder()
				.WithTitle(title)
				.WithSuccess()
				.WithFooter(new EmbedFooterBuilder
				{
					IconUrl = Context.User.GetAvatarUrl(),
					Text = Context.User.Username
				})
				.WithDescription(content ?? "");
		}
		protected EmbedBuilder CreateOkEmbed(string content = null) =>
			CreateOkEmbed("", content);

		protected EmbedBuilder CreateErrorEmbed(string title, string content)
		{
			return new EmbedBuilder()
				.WithTitle(title)
				.WithError()
				.WithFooter(new EmbedFooterBuilder
				{
					IconUrl = Context.User.GetAvatarUrl(),
					Text = Context.User.Username
				})
				.WithDescription(content ?? "");
		}
		protected EmbedBuilder CreateErrorEmbed(string content = null) =>
			CreateErrorEmbed("", content);

		protected EmbedBuilder CreateDefaultEmbed(string title, string content)
		{
			return new EmbedBuilder()
				.WithTitle(title)
				.WithDefaultColor()
				.WithDescription(content ?? "");
		}
		protected EmbedBuilder CreateDefaultEmbed(string content = null) =>
			CreateDefaultEmbed("", content);

		protected async Task<IUserMessage> SendOkAsync(string title, string content) =>
			await SendEmbedAsync(CreateOkEmbed(title, content));
		protected async Task<IUserMessage> SendOkAsync(string content = null) =>
			await SendOkAsync("", content);

		protected async Task<IUserMessage> SendErrorAsync(string title, string content) =>
			await SendEmbedAsync(CreateErrorEmbed(title, content));
		protected async Task<IUserMessage> SendErrorAsync(string content = null) =>
			await SendErrorAsync("", content);

		protected async Task<IUserMessage> SendDefaultEmbedAsync(string title, string content) =>
			await SendEmbedAsync(CreateDefaultEmbed(title, content));
		protected async Task<IUserMessage> SendDefaultEmbedAsync(string description) =>
			await SendDefaultEmbedAsync("", description);

		protected async Task<IUserMessage> SendEmbedAsync(Embed embed) =>
			await Context.ReplyAsync(embed).ConfigureAwait(false);
		protected async Task<IUserMessage> SendEmbedAsync(EmbedBuilder embed) =>
			await Context.ReplyAsync(embed).ConfigureAwait(false);
	}
}