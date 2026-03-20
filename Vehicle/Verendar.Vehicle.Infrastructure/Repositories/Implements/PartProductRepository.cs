using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class PartProductRepository(VehicleDbContext context) : PostgresRepository<PartProduct>(context), IPartProductRepository
    {
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
    }
}
