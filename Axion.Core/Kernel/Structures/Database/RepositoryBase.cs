using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;

namespace Axion.Kernel.Structures.Database
{
    public abstract class RepositoryBase<T> : Repository<T>, IRepository<T> where T : class, new()
    {
        public RepositoryBase(IConnect connect) : base(connect) { }
    }
}