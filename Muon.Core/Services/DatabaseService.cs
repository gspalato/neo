using System.Threading.Tasks;
using MongoDB.Driver;
using Muon.Kernel.Structures.Database;

namespace Muon.Services
{
	public interface IDatabaseService
	{
		void Initialize();
		Task<GuildSettings> GetGuildSettingsAsync(ulong guildId);
		Task<GuildSettings> CreateGuildSettingsAsync(ulong guildId, string prefix = "pls ");
	}

	public class DatabaseService : IDatabaseService
	{
		private IMongoClient _connection;
		private IMongoDatabase _database;

		public DatabaseService()
		{ }

		public void Initialize()
		{
			_connection = new MongoClient("mongodb://localhost:27017");
			_database = _connection.GetDatabase("muon");
		}

		public async Task<GuildSettings> GetGuildSettingsAsync(ulong guildId)
		{
			var collection = _database.GetCollection<GuildSettings>("GuildSettings");

			return await collection.Find(x => x.guild_id == guildId.ToString()).FirstOrDefaultAsync();
		}

		public async Task<GuildSettings> CreateGuildSettingsAsync(ulong guildId, string prefix = "pls ")
		{
			var collection = _database.GetCollection<GuildSettings>("GuildSettings");

			var settings = new GuildSettings
			{
				guild_id = guildId.ToString(),
				prefix = prefix
			};

			await collection.InsertOneAsync(settings);

			return settings;
		}
	}
}