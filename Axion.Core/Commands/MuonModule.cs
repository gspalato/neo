using Axion.Kernel.Utilities;
using Axion.Services;
using Discord;
using Qmmands;
using System.Threading.Tasks;

namespace Axion.Commands
{
	public abstract class AxionModule : ModuleBase<AxionContext>
	{
		public ICommandService CommandService { get; }
		public IMusicService MusicService { get; set; }

		protected async Task<IUserMessage> SendOkAsync(string content = null)
		{
			var embed = new EmbedBuilder()
			.WithSuccess()
			.WithFooter(new EmbedFooterBuilder
			{
				IconUrl = Context.User.GetAvatarUrl(),
				Text = Context.User.Username
			})
			.WithDescription(content ?? string.Empty);

			return await SendEmbedAsync(embed);
		}

		protected async Task<IUserMessage> SendErrorAsync(string content = null)
		{
			var embed = new EmbedBuilder()
			.WithError()
			.WithFooter(new EmbedFooterBuilder
			{
				IconUrl = Context.User.GetAvatarUrl(),
				Text = Context.User.Username
			})
			.WithDescription(content ?? string.Empty);

			return await SendEmbedAsync(embed);
		}

		protected async Task<IUserMessage> SendDefaultEmbedAsync(string title, string content)
		{
			var embed = new EmbedBuilder()
			.WithTitle(title)
			.WithDefaultColor()
			.WithDescription(content ?? string.Empty);

			return await SendEmbedAsync(embed);
		}
		protected async Task<IUserMessage> SendDefaultEmbedAsync(string description) =>
			await SendDefaultEmbedAsync("", description);

		protected async Task<IUserMessage> SendEmbedAsync(Embed embed) =>
			await Context.Channel.SendMessageAsync(embed: embed);
		protected async Task<IUserMessage> SendEmbedAsync(EmbedBuilder embed) =>
			await Context.Channel.SendMessageAsync(embed: embed.Build());
	}
}