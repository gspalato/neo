using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Spade.Core.Commands
{
	/*
	 * 
	 * This context has override parameters.
	 * They're meant to be used as a sudo system.
	 * 
	 * overrideUser (User): Changes the context's user to a specific one.
	 * overridePermissionsUser (PermissionsUser): User which permissions will be used to perform checks.
	 * 
	 * Every check (including check attributes) must always use the PermissionsUser to perform checks.
	 * 
	 */

	public class SpadeContext : CommandContext
	{
		public readonly ITextChannel Channel;
		public readonly DiscordSocketClient Client;
		public readonly IGuild Guild;
		public readonly bool IsSudo;
		public readonly IGuildUser Me;
		public readonly IUserMessage Message;
		public readonly DateTimeOffset Now;
		public readonly IGuildUser User;
		public readonly IGuildUser PermissionsUser;

		public SpadeContext(IUserMessage msg, IGuildUser me, IServiceProvider services,
			bool isSudo = false, IGuildUser overrideUser = null, IGuildUser overridePermissionsUser = null)
			: base(services)
		{
			Channel = msg.Channel as ITextChannel;
			Client = services.GetService<DiscordSocketClient>();
			Guild = Channel.Guild;
			IsSudo = isSudo;
			Me = me;
			Message = msg;
			Now = DateTimeOffset.UtcNow;
			User = overrideUser ?? (IGuildUser)msg.Author;
			PermissionsUser = overridePermissionsUser ?? User;
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
