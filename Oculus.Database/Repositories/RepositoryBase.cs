using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;

namespace Oculus.Database.Repositories
{
	public abstract class RepositoryBase<T> : Repository<T> where T : class, new()
	{
		protected RepositoryBase(IConnect connect) : base(connect) { }
	}
}