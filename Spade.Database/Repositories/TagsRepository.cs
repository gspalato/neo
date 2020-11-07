using Spade.Database.Entities;
using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;
using Discord;
using System.Threading.Tasks;

namespace Spade.Database.Repositories
{
	public interface ITagsRepository : IRepository<TagEntry>
	{
		Task<ITagEntry> CreateTagAsync(ulong guildId, ulong authorId, string name, string content);
		Task<ITagEntry> CreateTagAsync(IGuild guild, IUser author, string name, string content);

		Task DeleteTagAsync(ulong guildId, string name);
		Task DeleteTagAsync(IGuild guild, string name);

		Task<ITagEntry> EditTagAsync(ulong guildId, string name, string content);
		Task<ITagEntry> EditTagAsync(IGuild guild, string name, string content);

		Task<ITagEntry> GetTagAsync(ulong guildId, string name);
		Task<ITagEntry> GetTagAsync(IGuild guild, string name);

		Task<ITagEntry> UpdateTagUsage(ulong guildId, string name);
		Task<ITagEntry> UpdateTagUsage(IGuild guild, string name);
	}

	public sealed class TagsRepository : RepositoryBase<TagEntry>, ITagsRepository
	{
		public TagsRepository(IConnect connect) : base(connect) { }

		public async Task<ITagEntry> CreateTagAsync(ulong guildId, ulong authorId, string name, string content)
		{
			var tag = new TagEntry
			{
				GuildId = guildId.ToString(),
				Name = name,
				Content = content,
				Author = authorId.ToString()
			};

			return await AddAsync(tag);
		}
		public Task<ITagEntry> CreateTagAsync(IGuild guild, IUser author, string name, string content) =>
			CreateTagAsync(guild.Id, author.Id, name, content);

		public async Task DeleteTagAsync(ulong guildId, string name) =>
			await DeleteAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower());
		public Task DeleteTagAsync(IGuild guild, string name) => DeleteTagAsync(guild.Id, name);

		public async Task<ITagEntry> EditTagAsync(ulong guildId, string name, string content)
		{
            if (await GetTagAsync(guildId, name) is not TagEntry tag)
				return default;

			var update = MongoDB.Driver.Builders<TagEntry>.Update.Set("content", content);

			// Client prediction!
			var editedTag = tag with { Content = content };

			await UpdateAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower(), update);

			return editedTag;
		}
		public Task<ITagEntry> EditTagAsync(IGuild guild, string name, string content) => EditTagAsync(guild.Id, name, content);

		public async Task<ITagEntry> GetTagAsync(ulong guildId, string name) =>
			await FindAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower());
		public Task<ITagEntry> GetTagAsync(IGuild guild, string name) => GetTagAsync(guild.Id, name);

		public async Task<ITagEntry> UpdateTagUsage(ulong guildId, string name)
		{
			if (await GetTagAsync(guildId, name) is not TagEntry tag)
				return default;

			var update = MongoDB.Driver.Builders<TagEntry>.Update.Set("uses", tag.Uses + 1);

			// Client prediction!
			var editedTag = tag with { Uses = tag.Uses + 1 };

			await UpdateAsync(t => t.GuildId == guildId.ToString() && t.Name.ToLower() == name.ToLower(), update);

			return editedTag;
		}
		public Task<ITagEntry> UpdateTagUsage(IGuild guild, string name) => UpdateTagUsage(guild.Id, name);
	}
}