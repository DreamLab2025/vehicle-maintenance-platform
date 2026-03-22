using Microsoft.EntityFrameworkCore;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class ModelRepository(VehicleDbContext context) : PostgresRepository<Model>(context), IModelRepository
    {
        public IQueryable<Model> AsQueryableWithBrandAndVehicleType() =>
            _dbSet
                .Include(m => m.Brand)
                    .ThenInclude(b => b.VehicleType);

        public async Task<Model?> GetByIdWithBrandTypeAndVariantsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Brand).ThenInclude(b => b.VehicleType)
                .Include(m => m.Variants)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }
    }
}
