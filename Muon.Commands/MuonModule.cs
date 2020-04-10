
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Rest;

using Qmmands;

using Muon.Kernel.Utilities;
using Muon.Services;

namespace Muon.Commands
{
	public abstract class MuonModule : ModuleBase<MuonContext>
	{
		public IDatabaseService databaseService { get; set; }
		public ICommandService commandService { get; set; }
		public IMusicService musicService { get; set; }

		public MuonModule() : base()
		{
			
		}

		protected async Task<RestUserMessage> SendOkAsync(string content = null)
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

		protected async Task<RestUserMessage> SendErrorAsync(string content = null)
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

		protected async Task<RestUserMessage> SendDefaultEmbedAsync(string title, string content = null)
		{
			EmbedBuilder embed = new EmbedBuilder()
			.WithDefaultColor()
			.WithDescription(content ?? string.Empty);

			return await SendEmbedAsync(embed);
		}
		protected async Task<RestUserMessage> SendDefaultEmbedAsync(string description) =>
			await SendDefaultEmbedAsync("", description);

		protected async Task<RestUserMessage> SendEmbedAsync(Embed embed) =>
			await Context.Channel.SendMessageAsync(embed: embed);
		protected async Task<RestUserMessage> SendEmbedAsync(EmbedBuilder embed) =>
			await Context.Channel.SendMessageAsync(embed: embed.Build());
	}
}