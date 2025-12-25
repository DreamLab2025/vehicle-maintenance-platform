using System.Linq.Expressions;

namespace VMP.Common.Databases.Interfaces
{
    public interface IGenericRepository<T> where T : class, IEntity
    {
        Task<IReadOnlyCollection<T>> GetAllAsync();
        Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task<T?> GetByIdAsync(Guid id, Expression<Func<T, bool>> expression);
        Task<T?> GetByIdAsync(Guid id);

        Task<T?> FindOneAsync(Expression<Func<T, bool>> expression);
        Task<long> CountAsync(Expression<Func<T, bool>> expression);

        Task<T> AddAsync(T entity);
        Task UpdateAsync(Guid id, T entity);
        Task DeleteAsync(Guid id);
    }
}
