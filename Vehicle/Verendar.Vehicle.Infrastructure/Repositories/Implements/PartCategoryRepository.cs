using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class PartCategoryRepository(VehicleDbContext context) : PostgresRepository<PartCategory>(context), IPartCategoryRepository
    {
        public async Task<PartCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        }

        public async Task<IEnumerable<PartCategory>> GetActiveOrderedAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PartCategory>> GetByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken = default)
        {
            if (codes.Count == 0)
                return [];

            return await _dbSet
                .Where(pc => codes.Contains(pc.Code))
                .ToListAsync(cancellationToken);
        }
    }
}
