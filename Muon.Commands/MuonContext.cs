using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;

using Qmmands;

using Muon.Kernel.Utilities;

namespace Muon.Commands
{
	public class MuonContext : CommandContext
	{
		public readonly DiscordSocketClient Client;
		public readonly SocketGuild Guild;
		public readonly SocketTextChannel Channel;
		public readonly SocketGuildUser User;
		public readonly SocketUserMessage Message;
		public readonly DateTimeOffset Now;

		public MuonContext(SocketMessage msg, IServiceProvider services) : base(services)
		{
			Console.WriteLine(services.ToString());

			Client = services.GetRequiredService<DiscordSocketClient>();
			Guild = ((SocketTextChannel)msg.Channel)?.Guild;
			Channel = (SocketTextChannel)msg.Channel;
			User = (SocketGuildUser)msg.Author;
			Message = (SocketUserMessage)msg;
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

		public async Task<RestUserMessage> ReplyAsync(string content) =>
			await Channel.SendMessageAsync(content);

		public async Task<RestUserMessage> ReplyAsync(Embed embed) =>
			await Channel.SendMessageAsync(embed: embed);

		public async Task<RestUserMessage> ReplyAsync(EmbedBuilder embed) =>
		   await ReplyAsync(embed.Build());

		public Task ReactAsync(string unicode) => Message.AddReactionAsync(new Emoji(unicode));
	}
}
