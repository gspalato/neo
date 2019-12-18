using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;

using Qmmands;


namespace Arpa.Structures
{
	public interface ICommandContext
	{
		Task<DiscordMessage> ReplyAsync(string text = null, bool isTTS = false, DiscordEmbed embed = null);
	}

	public class ArpaCommandContext : CommandContext
	{
		public DiscordClient Client;
		public DiscordChannel Channel => Message.Channel;
		public DiscordGuild Guild => Message.Channel.Guild;
		public DiscordMessage Message { get; }
		public DiscordUser User => Message.Author;

		public ServiceProvider Services;

		public ArpaCommandContext(DiscordMessage message, IServiceProvider provider) : base(provider)
		{
			this.Message = message;
			this.Services = provider as ServiceProvider;
		}

		public async Task<DiscordMessage> ReplyAsync(string text = null, bool isTTS = false,
		DiscordEmbed embed = null)
		{
			return await this.Channel.SendMessageAsync(content: text, tts: isTTS, embed: embed);
		}
	}
}