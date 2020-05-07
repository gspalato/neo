using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Commands
{
	public class AxionContext : CommandContext
	{
		public readonly ITextChannel Channel;
		public readonly DiscordSocketClient Client;
		public readonly IGuild Guild;
        public readonly IGuildUser Me;
		public readonly IUserMessage Message;
		public readonly DateTimeOffset Now;
		public readonly IGuildUser User;

		public AxionContext(IUserMessage msg, IGuildUser me, IServiceProvider services) : base(services)
		{
			Channel = (ITextChannel)msg.Channel;
			Client = services.GetService<DiscordSocketClient>();
			Guild = Channel.Guild;
            Me = me;
			Message = msg;
			Now = DateTimeOffset.UtcNow;
			User = (IGuildUser)msg.Author;
		}

		public T GetService<T>() =>
			ServiceProvider.GetRequiredService<T>();

		public async Task<IUserMessage> ReplyAsync(string content, Embed embed) =>
			await Channel.SendMessageAsync(content, embed: embed);
		public async Task<IUserMessage> ReplyAsync(string content) =>
			await ReplyAsync(content, null);
		public async Task<IUserMessage> ReplyAsync(Embed embed) =>
			await ReplyAsync("", embed);
		public async Task<IUserMessage> ReplyAsync(EmbedBuilder embed) =>
			await ReplyAsync("", embed.Build());

		public Task ReactAsync(string unicode) =>
			Message.AddReactionAsync(new Emoji(unicode));
	}
}
