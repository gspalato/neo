using Axion.Core.Structures.Database;
using Canducci.MongoDB.Repository.Contracts;
using Canducci.MongoDB.Repository.Connection;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Axion.Core.Database.Entities;
using Discord;

namespace Axion.Core.Database
{
    public interface ITagsRepository : IRepository<Tag>
    {
        Task CreateTagAsync(ulong guildId, ulong authorId, string name, string content);
        Task CreateTagAsync(IGuild guild, IUser author, string name, string content);

        Task<Tag> GetTagAsync(ulong guildId, string name);
        Task<Tag> GetTagAsync(IGuild guild, string name);

        Task DeleteTagAsync(ulong guildId, string name);
        Task DeleteTagAsync(IGuild guild, string name);
    }

    public sealed class TagsRepository : RepositoryBase<Tag>, ITagsRepository
    {
        public TagsRepository(IConnect connect) : base(connect) { }

        public async Task CreateTagAsync(ulong guildId, ulong authorId, string name, string content)
        {
            var tag = new Tag
            {
                GuildId = guildId.ToString(),
                Name = name,
                Content = content,
                Author = authorId.ToString()
            };

            await AddAsync(tag);
        }

        public Task CreateTagAsync(IGuild guild, IUser author, string name, string content) =>
            CreateTagAsync(guild.Id, author.Id, name, content);

        public async Task<Tag> GetTagAsync(ulong guildId, string name) =>
            await FindAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower());

        public Task<Tag> GetTagAsync(IGuild guild, string name) =>
            GetTagAsync(guild.Id, name);

        public async Task DeleteTagAsync(ulong guildId, string name) =>
            await DeleteAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower());

        public Task DeleteTagAsync(IGuild guild, string name) =>
            DeleteTagAsync(guild.Id, name);
    }
}