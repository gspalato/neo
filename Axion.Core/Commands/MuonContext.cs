using Axion.Kernel.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Commands
{
	public class AxionContext : CommandContext
	{
		public readonly ITextChannel Channel;
		public readonly DiscordSocketClient Client;
		public readonly IGuild Guild;
		public readonly IUserMessage Message;
		public readonly DateTimeOffset Now;
		public readonly IGuildUser User;

		public AxionContext(IUserMessage msg, IServiceProvider services) : base(services)
		{
			Channel = (ITextChannel)msg.Channel;
			Client = services.GetService<DiscordSocketClient>();
			Guild = Channel.Guild;
			Message = msg;
			Now = DateTimeOffset.UtcNow;
			User = (IGuildUser)msg.Author;
		}

		public Embed CreateEmbed(string content) =>
			new EmbedBuilder()
			.WithSuccess()
			.WithAuthor(User)
			.WithDescription(content).Build();

		public EmbedBuilder CreateEmbedBuilder(string content = null) =>
			new EmbedBuilder()
			.WithSuccess()
			.WithAuthor(User)
			.WithDescription(content ?? string.Empty);

		public async Task<IUserMessage> ReplyAsync(string content) =>
			await Channel.SendMessageAsync(content);

		public async Task<IUserMessage> ReplyAsync(Embed embed) =>
			await Channel.SendMessageAsync(embed: embed);

		public async Task<IUserMessage> ReplyAsync(EmbedBuilder embed) =>
			await ReplyAsync(embed.Build());

		public Task ReactAsync(string unicode) =>
			Message.AddReactionAsync(new Emoji(unicode));
	}
}
