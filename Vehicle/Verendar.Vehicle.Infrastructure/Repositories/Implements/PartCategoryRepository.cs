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
                .Where(x => x.Status == Common.Databases.Base.EntityStatus.Active)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync(cancellationToken);
        }
    }
}
