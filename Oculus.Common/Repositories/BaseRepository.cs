using MongoDB.Driver;
using Oculus.Common.Data;
using Oculus.Common.Entities;

namespace Oculus.Common.Repositories
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetByPropertyAsync(string name, object value);
        Task<T?> GetFirstByPropertyAsync(string name, object value);
        Task<T> InsertAsync(T entity);
        Task<bool> RemoveAsync(string id);
    }

    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> Collection;

        public BaseRepository(IDatabaseContext dataContext)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            Collection = dataContext.GetCollection<T>(typeof(T).Name);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Collection.Find(_ => true).ToListAsync().ConfigureAwait(false);
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(_ => _.Id, id);

            return await Collection.Find(filter).FirstAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> GetByPropertyAsync(string name, object value)
        {
            var filter = Builders<T>.Filter.Eq(name, value);

            return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
        }

        public async Task<T?> GetFirstByPropertyAsync(string name, object value)
        {
            var filter = Builders<T>.Filter.Eq(name, value);

            return await Collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<T> InsertAsync(T entity)
        {
            await Collection.InsertOneAsync(entity).ConfigureAwait(false);

            return entity;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            var result = await Collection.DeleteOneAsync(Builders<T>.Filter.Eq(_ => _.Id, id)).ConfigureAwait(false);

            return result.DeletedCount > 0;
        }
    }
}
