using System;
using System.Linq;
using System.Threading.Tasks;

using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

using DSharpPlus;

using Muon.Core.Structures;

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
		IMongoClient connection;
		IMongoDatabase database;

		public DatabaseService()
		{ }

		public void Initialize()
		{
			this.connection = new MongoClient("mongodb://localhost:27017");
			this.database = this.connection.GetDatabase("muon");
		}

		public async Task<GuildSettings> GetGuildSettingsAsync(ulong guildId)
		{
			IMongoCollection<GuildSettings> collection = database.GetCollection<GuildSettings>("GuildSettings");

			return await collection.Find(x => x.guild_id == guildId.ToString()).FirstOrDefaultAsync();
		}

		public async Task<GuildSettings> CreateGuildSettingsAsync(ulong guildId, string prefix = "pls ")
		{
			IMongoCollection<GuildSettings> collection = database.GetCollection<GuildSettings>("GuildSettings");

			GuildSettings settings = new GuildSettings
			{
				guild_id = guildId.ToString(),
				prefix = prefix
			};

			await collection.InsertOneAsync(settings);

			return settings;
		}
	}
}