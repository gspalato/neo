using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace Arpa.Structures
{
	public interface I_CommandContext
	{
		Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null);
	}

	public class _CommandContext : I_CommandContext
	{
		public IDiscordClient Client;
		public ITextChannel Channel;
		public IGuild Guild;
		public IUserMessage Message;
		public IUser User;

		public _CommandContext(IDiscordClient client, IUserMessage msg)
		{
			this.Client = client;
			this.Channel = msg.Channel as ITextChannel;
			this.Guild = (msg.Channel as IGuildChannel)?.Guild;
			this.Message = msg;
			this.User = msg.Author;
		}

		public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null)
		{
			return await this.Channel.SendMessageAsync(text: text, isTTS: isTTS, embed: embed);
		}
	}
}