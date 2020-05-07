using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Axion.Core.Structures.Interactivity
{
	public class ReactionAwaiter : EventAwaiter<SocketReaction>
	{
		public readonly IUserMessage Message;

		private readonly bool _shouldDeleteReaction;

		public ReactionAwaiter(DiscordSocketClient client, IUserMessage message,
            Predicate<SocketReaction> filter, bool shouldDeleteReaction = true) : base(client, filter)
		{
			Message = message;

			_shouldDeleteReaction = shouldDeleteReaction;
		}

		public override Task<SocketReaction> Wait(int millisecondsTimeout = 180000)
		{
			Client.ReactionAdded += HandleEvent;

			return base.Wait(millisecondsTimeout);
		}

		private async Task HandleEvent(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!(channel is ITextChannel textChannel))
				return;

            var me = await textChannel.Guild.GetCurrentUserAsync();
			if (reaction.UserId == me.Id)
				return;

			if (reaction.MessageId != Message.Id)
				return;

			if (Filter(reaction))
			{
				Tcs.SetResult(reaction);
				if (_shouldDeleteReaction)
					await Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

				Dispose();
			}
		}

		public override void Dispose()
		{
			Client.ReactionAdded -= HandleEvent;
			base.Dispose();
		}

		~ReactionAwaiter()
		{
			Dispose();
		}
	}
}
