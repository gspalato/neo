using System;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using Discord;

namespace Arpa.Services
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

		public void GetGuildSettingsAsync()
		{

		}
	}
}