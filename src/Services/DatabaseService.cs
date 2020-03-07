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
	}

	public class DatabaseService : IDatabaseService
	{
		IMongoClient connection;

		public DatabaseService()
		{

		}

		public void Initialize()
		{
			IMongoClient connection = new MongoClient("mongodb://localhost:27017");
			this.connection = connection;
		}

		public async Task<GuildSettings> GetGuildSettingsAsync(ulong guildId)
		{
			IMongoDatabase db = this.connection.GetDatabase("arpa");
			IMongoCollection<GuildSettings> collection = db.GetCollection<GuildSettings>("GuildSettings");

			return await collection.Find(x => x.guild_id == guildId.ToString()).FirstOrDefaultAsync();
		}

		public async Task<GuildSettings> CreateGuildSettingsAsync(ulong guildId, string prefix = "pls ")
		{
			IMongoDatabase db = this.connection.GetDatabase("arpa");
			IMongoCollection<GuildSettings> collection = db.GetCollection<GuildSettings>("GuildSettings");

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