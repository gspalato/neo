using Spade.Core.Structures.Attributes;
using Spade.Database.Repositories;
using Discord;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spade.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Group("tag")]
	public class Tag : SpadeModule
	{
		public ITagsRepository TagsRepository { get; set; }

		[Command]
		public async Task ExecuteAsync([Remainder] string name)
		{
			var tag = await TagsRepository.GetTagAsync(Context.Guild, name);
			if (tag is null)
			{
				await Context.ReplyAsync($"Tag \"{name}\" doesn't exist.");
				return;
			}

			_ = TagsRepository.UpdateTagUsage(Context.Guild, name);
			await Context.ReplyAsync(tag.Content);
		}

		[Command("create")]
		public async Task CreateAsync(string name, [Remainder] string content)
		{
			if (name.Length > 25)
			{
				await Context.ReplyAsync("Tag name's too long. Please make it below 25 characters.");
				return;
			}

			var tag = await TagsRepository.GetTagAsync(Context.Guild, name);
			if (tag != null)
			{
				await Context.ReplyAsync($"Tag \"{name}\" already exists.");
				return;
			}

			var disallowed = new[] { "create", "update", "delete", "info" };
			if (disallowed.Contains(name) || name.Contains('`'))
			{
				await Context.ReplyAsync($"You can't create a tag with this name.");
				return;
			}

			await TagsRepository.CreateTagAsync(Context.Guild, Context.User, name, content);

			await Context.ReplyAsync($"Tag \"{name}\" created.");
		}

		[Command("delete")]
		public async Task DeleteAsync([Remainder] string name)
		{
			var tag = await TagsRepository.GetTagAsync(Context.Guild, name);
			if (tag is null)
			{
				await Context.ReplyAsync($"Tag \"{name}\" doesn't exist.");
				return;
			}

			if (tag.Author != Context.User.Id.ToString())
			{
				if (!Context.User.GuildPermissions.Has(GuildPermission.Administrator))
				{
					await Context.ReplyAsync("You can't delete someone else's tag.");
					return;
				}
			}

			await TagsRepository.DeleteTagAsync(Context.Guild, name);

			await Context.ReplyAsync($"Tag \"{name}\" deleted.");
		}

		[Command("update", "edit")]
		public async Task EditAsync(string name, [Remainder] string content)
		{
			var tag = await TagsRepository.GetTagAsync(Context.Guild, name);
			if (tag is null)
			{
				await Context.ReplyAsync($"Tag \"{name}\" doesn't exist.");
				return;
			}

			if (tag.Author != Context.User.Id.ToString())
			{
				await Context.ReplyAsync("You can't edit someone else's tag.");
				return;
			}

			await TagsRepository.EditTagAsync(Context.Guild, name, content);
			await Context.ReplyAsync($"Tag \"{name}\" edited.");
		}

		[Command("info")]
		public async Task InfoAsync([Remainder] string name)
		{
			var tag = await TagsRepository.GetTagAsync(Context.Guild, name);
			if (tag is null)
			{
				await Context.ReplyAsync($"Tag \"{name}\" doesn't exist.");
				return;
			}

			var author = Context.Client.GetUser(Convert.ToUInt64(tag.Author));

			var embed = CreateDefaultEmbed(@"\💬 tag info", "")
				.AddField("Name", Format.Code(name), true)
				.AddField("Uses", tag.Uses, true)
				.AddField("Author", author?.Mention ?? Format.Code(tag.Author));

			await SendEmbedAsync(embed);
		}
	}
}
