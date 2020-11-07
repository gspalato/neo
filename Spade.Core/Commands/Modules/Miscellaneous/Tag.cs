using Spade.Core.Structures.Attributes;
using Spade.Database.Repositories;
using Discord;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Spade.Core.Services;
using Spade.Database.Entities;

namespace Spade.Core.Commands.Modules.Miscellaneous
{
	[Category(Category.Miscellaneous)]
	[Group("tag")]
	public class Tag : SpadeModule
	{
		public ICacheManagerService CacheManagerService { get; set; }
		public ILoggingService LoggingService { get; set; }
		public ITagsRepository TagsRepository { get; set; }

		[Command]
		public async Task ExecuteAsync([Remainder] string name)
		{
			ITagEntry tag;

			string key = CacheManagerService.Format<TagEntry>(Context.Guild.Id, 0, name);
			if (CacheManagerService.IsSet(key))
				tag = CacheManagerService.Get<TagEntry>(key);
			else
			{
				tag = await TagsRepository.GetTagAsync(Context.Guild, name);

				if (!CacheManagerService.IsSet(key) && tag is not null)
				{
					LoggingService.Debug($"Appended tag {tag.Name} from guild {tag.GuildId} to cache");
					CacheManagerService.Set(key, tag);
				}
			}

			if (tag is null)
			{
				await Context.ReplyAsync($"Tag \"{name}\" doesn't exist.");
				return;
			}

			ITagEntry updatedTag = await TagsRepository.UpdateTagUsage(Context.Guild, name);
			CacheManagerService.Set(key, updatedTag);

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

			if (await TagsRepository.GetTagAsync(Context.Guild, name) is not null)
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

			ITagEntry fetchedTag = await TagsRepository.CreateTagAsync(Context.Guild, Context.User, name, content);

			string cacheKey = CacheManagerService.Format<TagEntry>(Context.Guild.Id, Context.User.Id, name);
			CacheManagerService.Set(cacheKey, fetchedTag);

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
				if (!Context.PermissionsUser.GuildPermissions.Has(GuildPermission.Administrator))
				{
					await Context.ReplyAsync("You can't delete someone else's tag.");
					return;
				}
			}

			await TagsRepository.DeleteTagAsync(Context.Guild, name);
			string cacheKey = CacheManagerService.Format<TagEntry>(Context.Guild.Id, Context.User.Id, name);
			CacheManagerService.Remove(cacheKey);

			await Context.ReplyAsync($"Tag \"{name}\" deleted.");
		}

		[Command("update", "edit")]
		public async Task EditAsync(string name, [Remainder] string content)
		{
			if (await TagsRepository.GetTagAsync(Context.Guild, name) is not ITagEntry tag)
			{
				await Context.ReplyAsync($"Tag \"{name}\" doesn't exist.");
				return;
			}

			if (tag.Author != Context.User.Id.ToString())
			{
				await Context.ReplyAsync("You can't edit someone else's tag.");
				return;
			}

			ITagEntry edited = await TagsRepository.EditTagAsync(Context.Guild, name, content);

			string cacheKey = CacheManagerService.Format<TagEntry>(Context.Guild.Id, Context.User.Id, name);
			CacheManagerService.Set(cacheKey, edited);

			await Context.ReplyAsync($"Tag \"{name}\" edited.");
		}

		[Command("info")]
		public async Task InfoAsync([Remainder] string name)
		{
			ITagEntry tag;

			string key = CacheManagerService.Format<TagEntry>(Context.Guild.Id, 0, name);
			if (CacheManagerService.IsSet(key))
				tag = CacheManagerService.Get<TagEntry>(key);
			else
			{
				tag = await TagsRepository.GetTagAsync(Context.Guild, name);

				if (!CacheManagerService.IsSet(key) && tag is not null)
				{
					LoggingService.Debug($"Appended tag {tag.Name} from guild {tag.GuildId} to cache");
					CacheManagerService.Set(key, tag);
				}
			}

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
