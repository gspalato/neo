using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Muon.Kernel.Utilities;
using Qmmands;

namespace Muon.Commands
{
	public class MuonContext : CommandContext
	{
		public readonly DiscordSocketClient Client;
		public readonly IGuild Guild;
		public readonly ITextChannel Channel;
		public readonly IGuildUser User;
		public readonly IUserMessage Message;
		public readonly DateTimeOffset Now;

		public MuonContext(IMessage msg, IServiceProvider services) : base(services)
		{
			Client = services.GetRequiredService<DiscordSocketClient>();
			Guild = ((ITextChannel)msg.Channel)?.Guild;
			Channel = (ITextChannel)msg.Channel;
			User = (IGuildUser)msg.Author;
			Message = (IUserMessage)msg;
			Now = DateTimeOffset.UtcNow;
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

		public Task ReactAsync(string unicode) => Message.AddReactionAsync(new Emoji(unicode));
	}
}
