using Microsoft.EntityFrameworkCore;
using Verender.Common.Databases.Implements;
using Verender.Vehicle.Domain.Entities;
using Verender.Vehicle.Domain.Repositories.Interfaces;
using Verender.Vehicle.Infrastructure.Data;

namespace Verender.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleVariantRepository : PostgresRepository<VehicleVariant>, IVehicleVariantRepository
    {
        public VehicleVariantRepository(VehicleDbContext context) : base(context)
        {
        }

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
