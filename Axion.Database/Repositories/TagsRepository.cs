using Axion.Database.Entities;
using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;
using Discord;
using System.Threading.Tasks;

namespace Axion.Database.Repositories
{
	public interface ITagsRepository : IRepository<Tag>
	{
		Task CreateTagAsync(ulong guildId, ulong authorId, string name, string content);
		Task CreateTagAsync(IGuild guild, IUser author, string name, string content);

		Task DeleteTagAsync(ulong guildId, string name);
		Task DeleteTagAsync(IGuild guild, string name);

		Task EditTagAsync(ulong guildId, string name, string content);
		Task EditTagAsync(IGuild guild, string name, string content);

		Task<Entities.ITag> GetTagAsync(ulong guildId, string name);
		Task<Entities.ITag> GetTagAsync(IGuild guild, string name);

		Task UpdateTagUsage(ulong guildId, string name);
		Task UpdateTagUsage(IGuild guild, string name);
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

		public async Task DeleteTagAsync(ulong guildId, string name) =>
			await DeleteAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower());
		public Task DeleteTagAsync(IGuild guild, string name) => DeleteTagAsync(guild.Id, name);

		public async Task EditTagAsync(ulong guildId, string name, string content)
		{
			var tag = await GetTagAsync(guildId, name);
			if (tag is null)
				return;

			var update = MongoDB.Driver.Builders<Tag>.Update.Set("content", content);

			await UpdateAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower(), update);
		}
		public Task EditTagAsync(IGuild guild, string name, string content) => EditTagAsync(guild.Id, name, content);

		public async Task<Entities.ITag> GetTagAsync(ulong guildId, string name) =>
			await FindAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower());
		public Task<Entities.ITag> GetTagAsync(IGuild guild, string name) => GetTagAsync(guild.Id, name);

		public async Task UpdateTagUsage(ulong guildId, string name)
		{
			var tag = await GetTagAsync(guildId, name);
			if (tag is null)
				return;

			var update = MongoDB.Driver.Builders<Tag>.Update.Set("uses", tag.Uses + 1);

			await UpdateAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower(), update);
		}
		public Task UpdateTagUsage(IGuild guild, string name) => UpdateTagUsage(guild.Id, name);
	}
}