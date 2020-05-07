using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Axion.Core.Structures.Interactivity
{
	using Page = Action<EmbedBuilder>;
	using ReactionBehavior = ValueTuple<Emoji, Action<PaginatedContext>>;

	public enum PaginatedBehavior
	{
		First,
		Previous,
		Delete,
		Next,
		Last
	}

	public class PaginatedMessageBuilder
    {
        public Func<EmbedBuilder> Template { get; private set; }
		public TimeSpan Timeout { get; private set; } = TimeSpan.FromMinutes(3);

		private readonly List<ReactionBehavior> _callbacks = new List<ReactionBehavior>();
		private readonly List<Page> _pages = new List<Page>();
		private IUser _responsible;

		public PaginatedMessageBuilder AddPage(Action<EmbedBuilder> builder)
		{
			_pages.Add(builder);

			return this;
		}

		public PaginatedMessageBuilder AddButton(Emoji emoji, Action<PaginatedContext> action)
		{
			_callbacks.Add((emoji, action));

			return this;
		}

		public PaginatedMessageBuilder AddButton(Emoji emoji, PaginatedBehavior eBehavior)
		{
            Action<PaginatedContext> action = eBehavior switch
			{
				PaginatedBehavior.First => 
					async ctx =>
					{
						await ctx.PaginatedMessage.SkipToPageAsync(0);
						await ctx.PaginatedMessage.Message.RemoveReactionAsync(ctx.Reaction.Emote, ctx.User);
					},

				PaginatedBehavior.Previous =>
					async ctx =>
					{
						var pm = ctx.PaginatedMessage;

						await pm.Message.RemoveReactionAsync(ctx.Reaction.Emote, ctx.User);

						if (pm.CurrentPage <= 0)
							return;

						await pm.SkipToPageAsync(pm.CurrentPage - 1);
					},

				PaginatedBehavior.Delete =>
					async ctx =>
					{
						await ctx.PaginatedMessage.Message.DeleteAsync();
						ctx.PaginatedMessage.Dispose();
					},

				PaginatedBehavior.Next =>
					async ctx =>
					{
						var pm = ctx.PaginatedMessage;

						await pm.Message.RemoveReactionAsync(ctx.Reaction.Emote, ctx.User);

						if (pm.CurrentPage >= pm.Pages.Length - 1)
							return;

						await pm.SkipToPageAsync(pm.CurrentPage + 1);
					},

				PaginatedBehavior.Last =>
					async ctx =>
					{
						var pm = ctx.PaginatedMessage;

						await pm.Message.RemoveReactionAsync(ctx.Reaction.Emote, ctx.User);

						if (pm.CurrentPage == pm.Pages.Length - 1)
							return;

						await pm.SkipToPageAsync(pm.Pages.Length - 1);
					},

				_ => default
			};

            _callbacks.Add((emoji, action));

			return this;

		}

		public PaginatedMessageBuilder WithDefaultButtons()
		{
			AddButton(new Emoji("⏪"), PaginatedBehavior.First);
			AddButton(new Emoji("◀️"), PaginatedBehavior.Previous);
			AddButton(new Emoji("⏹️"), PaginatedBehavior.Delete);
			AddButton(new Emoji("▶️"), PaginatedBehavior.Next);
			AddButton(new Emoji("⏩"), PaginatedBehavior.Last);

			return this;
		}

		public PaginatedMessageBuilder WithSimpleButtons()
		{
			AddButton(new Emoji("◀️"), PaginatedBehavior.Previous);
			AddButton(new Emoji("⏹️"), PaginatedBehavior.Delete);
			AddButton(new Emoji("▶️"), PaginatedBehavior.Next);

			return this;
		}

		public PaginatedMessageBuilder WithResponsible(IUser user)
		{
			_responsible = user;

			return this;
		}

		public PaginatedMessageBuilder WithTemplate(Func<EmbedBuilder> template)
		{
			Template = template;

			return this;
		}

		public PaginatedMessageBuilder WithTimeout(TimeSpan timeout)
		{
			Timeout = timeout;

			return this;
		}

		public IPaginatedMessage Build(DiscordSocketClient client)
		{
			var builtPages = new List<Embed>();

			foreach (var factory in _pages)
			{
				var embed = Template is null ? new EmbedBuilder() : Template();
				factory.Invoke(embed);
				builtPages.Add(embed.Build());
			}

			return new PaginatedMessage(client, _responsible, builtPages.ToArray(), _callbacks, (int)Timeout.TotalMilliseconds);
		}
	}
}
