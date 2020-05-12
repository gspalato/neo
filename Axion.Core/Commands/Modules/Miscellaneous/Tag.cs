using Axion.Core.Database;
using Axion.Core.Structures.Attributes;
using Discord;
using Qmmands;
using System.Threading.Tasks;

namespace Axion.Core.Commands.Modules.Miscellaneous
{
    [Category(Category.Miscellaneous)]
    [Group("tag")]
    public class Tag : AxionModule
    {
        public ITagsRepository TagsRepository { get; set; }

        [Command]
        public async Task ExecuteAsync(string name)
        {
            var tag = await TagsRepository.GetTagAsync(Context.Guild, name);
            if (tag is null)
            {
                await Context.ReplyAsync($"Tag \"{name}\" doesn't exist.");
                return;
            }

            await Context.ReplyAsync(tag.Content);
        }

        [Command("create")]
        public async Task CreateAsync(string name, [Remainder] string content)
        {
            var tag = await TagsRepository.GetTagAsync(Context.Guild, name);
            if (tag != null)
            {
                await Context.ReplyAsync($"Tag \"{name}\" already exists.");
                return;
            }

            await TagsRepository.CreateTagAsync(Context.Guild, Context.User, name, content);

            await Context.ReplyAsync($"Tag \"{name}\" created.");
        }

        [Command("delete")]
        public async Task DeleteAsync(string name)
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
    }
}
