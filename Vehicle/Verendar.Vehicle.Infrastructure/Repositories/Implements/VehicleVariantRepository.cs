using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleVariantRepository(VehicleDbContext context) : PostgresRepository<VehicleVariant>(context), IVehicleVariantRepository
    {
        public async Task<IEnumerable<VehicleVariant>> GetImagesByVehicleModelIdAsync(Guid vehicleModelId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(vi => vi.VehicleModelId == vehicleModelId)
                .OrderBy(vi => vi.Color)
                .ToListAsync(cancellationToken);
        }

        public async Task<VehicleVariant?> GetImageByVehicleModelIdAndColorAsync(Guid vehicleModelId, string color, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(vi => vi.VehicleModelId == vehicleModelId && vi.Color == color, cancellationToken);
        }
    }
}
