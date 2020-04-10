using Discord;
using Muon.Kernel.Utilities;
using Muon.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Muon.Commands
{
	public abstract class MuonModule : ModuleBase<MuonContext>
	{
		public IDatabaseService databaseService { get; set; }
		public ICommandService commandService { get; set; }
		public IMusicService musicService { get; set; }

		public MuonModule() : base()
		{ }

		protected async Task<IUserMessage> SendOkAsync(string content = null)
		{
			EmbedBuilder embed = new EmbedBuilder()
			.WithSuccess()
			.WithFooter(new EmbedFooterBuilder()
			{
				IconUrl = Context.User.GetAvatarUrl(),
				Text = Context.User.Username
			})
			.WithDescription(content ?? string.Empty);

			return await SendEmbedAsync(embed);
		}

		protected async Task<IUserMessage> SendErrorAsync(string content = null)
		{
			EmbedBuilder embed = new EmbedBuilder()
			.WithError()
			.WithFooter(new EmbedFooterBuilder()
			{
				IconUrl = Context.User.GetAvatarUrl(),
				Text = Context.User.Username
			})
			.WithDescription(content ?? string.Empty);

			return await SendEmbedAsync(embed);
		}

		protected async Task<IUserMessage> SendDefaultEmbedAsync(string title, string content = null)
		{
			EmbedBuilder embed = new EmbedBuilder()
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