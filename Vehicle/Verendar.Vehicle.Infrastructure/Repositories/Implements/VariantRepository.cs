using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class VariantRepository(VehicleDbContext context) : PostgresRepository<Variant>(context), IVariantRepository
    {
        public async Task<IEnumerable<Variant>> GetImagesByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(vi => vi.VehicleModelId == vehicleModelId)
                .OrderBy(vi => vi.Color)
                .ToListAsync(cancellationToken);
        }

        public async Task<Variant?> GetImageByVehicleModelIdAndColorAsync(Guid vehicleModelId, string color, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(vi => vi.VehicleModelId == vehicleModelId && vi.Color == color, cancellationToken);
        }
    }
}
