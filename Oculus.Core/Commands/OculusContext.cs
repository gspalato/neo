using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Oculus.Core.Commands
{
	/*
	 * 
	 * This context has override parameters.
	 * They're meant to be used as a sudo system.
	 * 
	 * OverrideUser (User): Changes the context's user to a specific one.
	 * OverridePermissionsUser (PermissionsUser): Set the user whose permissions will be used to perform checks.
	 * 
	 * Every check (including check attributes) must always use the PermissionsUser to perform checks.
	 * 
	 */

	public class OculusContext : CommandContext
	{
		public ITextChannel Channel { get; private set; }
		public DiscordSocketClient Client { get; private set; }
		public IGuild Guild { get; private set; }
		public IGuildUser Me { get; private set; }
		public IUserMessage Message { get; private set; }
		public DateTimeOffset Now { get; private set; }
		public IGuildUser User { get; private set; }
		public IGuildUser PermissionsUser { get; private set; }

		public bool ShouldInlineReply { get; private set; } = true;

		public OculusContext(IUserMessage msg, IGuildUser me, IServiceProvider services) : base(services)
		{
			Channel = msg.Channel as ITextChannel;
			Client = services.GetService<DiscordSocketClient>();
			Guild = Channel.Guild;
			Me = me;
			Message = msg;
			Now = DateTimeOffset.UtcNow;
			User = (IGuildUser)msg.Author;
			PermissionsUser = User;
		}

		public T GetService<T>() =>
			ServiceProvider.GetRequiredService<T>();

		public void SetReply(bool should = true)
			=> ShouldInlineReply = should;

		public void OverrideUser(IGuildUser user) =>
			User = user;
		public void OverridePermissionsUser(IGuildUser user) =>
			PermissionsUser = user;

		public async Task<IUserMessage> ReplyAsync(string content, Embed embed, bool reply = true)
        {
			MessageReference reference = default;
			if (!ShouldInlineReply)
				reference = null;
			else if (reply)
				reference = Message.Reference;
			
			return await Channel.SendMessageAsync(content, embed: embed, messageReference: reference);
		}

		public async Task<IUserMessage> ReplyAsync(string content, bool reply = true) =>
			await ReplyAsync(content, null, reply: reply);
		public async Task<IUserMessage> ReplyAsync(Embed embed, bool reply = true) =>
			await ReplyAsync("", embed, reply: reply);
		public async Task<IUserMessage> ReplyAsync(EmbedBuilder embed, bool reply = true) =>
			await ReplyAsync("", embed.Build(), reply: reply);

		public Task ReactAsync(string unicode) =>
			Message.AddReactionAsync(new Emoji(unicode));
	}
}
