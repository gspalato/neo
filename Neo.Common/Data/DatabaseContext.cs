using MongoDB.Driver;
using Neo.Common.Configurations;

namespace Neo.Common.Data
{
    public interface IDatabaseContext
    {
        MongoClient GetClient();
        IMongoCollection<T> GetCollection<T>(string name);
    }

    public class DatabaseContext : IDatabaseContext
    {
        private readonly MongoClient Client;
        private readonly IMongoDatabase Database;

        public DatabaseContext(BaseConfiguration config)
        {
            Console.WriteLine($"Connecting to {config.Database.Url} and database {config.Database.Name}...");
            Client = new MongoClient(new MongoUrl(config.Database.Url));
            Database = Client.GetDatabase(config.Database.Name);
        }

        public MongoClient GetClient()
        {
            return Client;
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return Database.GetCollection<T>(name);
        }
    }
}