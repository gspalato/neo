using System.Collections.Generic;
using System.Linq;
using Spade.Database.Entities;
using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;
using Discord;
using System.Threading.Tasks;
using Victoria;
using Victoria.Interfaces;

namespace Spade.Database.Repositories
{
	public interface IQueueRepository : IRepository<Queue>
	{
		Task<Queue> GetQueueAsync(ulong guildId, string name);
		Task<Queue> GetQueueAsync(IGuild guild, string name);
		
		Task SaveQueueAsync(ulong guildId, string name, DefaultQueue<LavaTrack> queue);
		Task SaveQueueAsync(IGuild guild, string name, DefaultQueue<LavaTrack> queue);
	}

	public sealed class QueueRepository : RepositoryBase<Queue>, IQueueRepository
	{
		public QueueRepository(IConnect connect) : base(connect) { }

		public async Task<Queue> GetQueueAsync(ulong guildId, string name)
			=> await FindAsync(q => q.GuildId == guildId.ToString() && q.Name.ToLower() == name.ToLower());

		public Task<Queue> GetQueueAsync(IGuild guild, string name)
			=> GetQueueAsync(guild.Id, name);
		
		public async Task SaveQueueAsync(ulong guildId, string name, DefaultQueue<LavaTrack> queue)
		{
			var queueModel = new Queue
			{
				Name = name,
				GuildId = guildId.ToString(),
				Urls = queue.Items.Select(t => ((LavaTrack)t).Url)
			};

			await AddAsync(queueModel);
		}

		public Task SaveQueueAsync(IGuild guild, string name, DefaultQueue<LavaTrack> queue)
			=> SaveQueueAsync(guild.Id, name, queue);
	}
}