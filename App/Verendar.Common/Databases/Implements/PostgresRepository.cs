using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Verendar.Common.Databases.Interfaces;

namespace Verendar.Common.Databases.Implements
{
    public class PostgresRepository<T>(DbContext context) : IGenericRepository<T> where T : class, IEntity
    {
        protected readonly DbSet<T> _dbSet = context.Set<T>();
        protected readonly DbContext _context = context;

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>> expression)
        {
            var count = await _dbSet.CountAsync(expression);
            return count;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                if (entity is ISoftDeleteEntity softDeleteEntity)
                {
                    softDeleteEntity.DeletedAt = DateTime.UtcNow;
                    _context.Entry(entity).State = EntityState.Modified;
                }
                else
                {
                    _dbSet.Remove(entity);
                }
            }
        }

        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.FirstOrDefaultAsync(expression);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id, Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateAsync(Guid id, T entity)
        {
            var existingEntity = await _dbSet.FindAsync(id);
            if (existingEntity != null)
            {
                if (!ReferenceEquals(existingEntity, entity))
                {
                    _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                }
            }
        }

        public IQueryable<T> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            int totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
