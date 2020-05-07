using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;

namespace Axion.Core.Structures.Database
{
    public abstract class RepositoryBase<T> : Repository<T>, IRepository<T> where T : class, new()
    {
        protected RepositoryBase(IConnect connect) : base(connect) { }
    }
}