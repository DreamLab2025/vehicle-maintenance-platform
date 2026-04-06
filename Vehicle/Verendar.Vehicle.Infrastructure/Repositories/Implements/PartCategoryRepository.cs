using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class PartCategoryRepository(VehicleDbContext context) : PostgresRepository<PartCategory>(context), IPartCategoryRepository
    {
        public async Task<PartCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        }

        public async Task<IEnumerable<PartCategory>> GetActiveOrderedAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PartCategory>> GetBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken = default)
        {
            if (slugs.Count == 0)
                return [];

            return await _dbSet
                .Where(pc => slugs.Contains(pc.Slug))
                .ToListAsync(cancellationToken);
        }
    }
}
