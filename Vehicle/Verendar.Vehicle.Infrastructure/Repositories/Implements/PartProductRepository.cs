using Microsoft.EntityFrameworkCore;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class PartProductRepository(VehicleDbContext context) : PostgresRepository<PartProduct>(context), IPartProductRepository
    {
        public IQueryable<PartProduct> AsQueryableWithCategory() =>
            _dbSet.Include(p => p.Category);

        public async Task<PartProduct?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, cancellationToken);
        }

        public async Task<IEnumerable<PartProduct>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.PartCategoryId == categoryId)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PartProduct>> GetByBrandAsync(string brand, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.Brand == brand)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PartProduct>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids.Count == 0)
                return [];

            return await _dbSet
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
