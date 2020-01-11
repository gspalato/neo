using System;
using System.Linq;
using System.Threading.Tasks;

using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

using DSharpPlus;

using Arpa.Structures;

namespace Arpa.Services
{
	public class DatabaseService : IDatabaseService
	{
		IMongoClient connection;

		public DatabaseService()
		{

		}

		public void Initialize()
		{
			IMongoClient connection = new MongoClient(new MongoClientSettings
			{
				Server = new MongoServerAddress("localhost", 27017),
				UseTls = true
			});
			this.connection = connection;
		}

		public GuildSettings GetGuildSettingsAsync(ulong guildId)
		{
			IMongoDatabase db = this.connection.GetDatabase("arpa");
			IMongoCollection<GuildSettings> collection = db.GetCollection<GuildSettings>("GuildSettings");

			var filter = Builders<GuildSettings>.Filter.Eq("guild_id", guildId);
			var found = collection.Find(filter).FirstOrDefault();

			return found;
		}
	}
}