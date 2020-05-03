using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public class ReactionAwaiter : EventAwaiter<SocketReaction>, IEventAwaiter<SocketReaction>
	{
		public readonly IUserMessage Message;

		private bool _shouldDeleteReaction;

		public ReactionAwaiter(DiscordSocketClient client,
			IUserMessage message, Func<SocketReaction, bool> filter,
			bool shouldDeleteReaction = true) : base(client, filter)
		{
			Message = message;

			_shouldDeleteReaction = shouldDeleteReaction;
		}

		public override Task<SocketReaction> Wait(int millisecondsTimeout = 180000)
		{
			_client.ReactionAdded += HandleEvent;

			return base.Wait(millisecondsTimeout);
		}

		private async Task HandleEvent(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!(channel is ITextChannel textChannel))
				return;

			IUserMessage message;
			if (cache.HasValue)
				message = cache.Value;
			else if (reaction.Message.IsSpecified)
				message = reaction.Message.Value;
			else
				message = await channel.GetMessageAsync(reaction.MessageId) as IUserMessage;

			var me = await textChannel.Guild.GetCurrentUserAsync();
			if (reaction.UserId == me.Id)
				return;

			if (reaction.MessageId != Message.Id)
				return;

			if (Filter(reaction))
			{
				_tcs.SetResult(reaction);
				if (_shouldDeleteReaction)
					await Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

				Dispose();
			}
		}

		public override void Dispose()
		{
			_client.ReactionAdded -= HandleEvent;
			base.Dispose();
		}

		~ReactionAwaiter()
		{
			Dispose();
		}
	}
}
