using Microsoft.EntityFrameworkCore;
using Verender.Common.Databases.Implements;
using Verender.Vehicle.Domain.Entities;
using Verender.Vehicle.Domain.Repositories.Interfaces;
using Verender.Vehicle.Infrastructure.Data;

namespace Verender.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleBrandRepository : PostgresRepository<VehicleBrand>, IVehicleBrandRepository
    {
        public VehicleBrandRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<VehicleBrand?> GetByIdWithTypesAsync(Guid id)
        {
            return await _context.Set<VehicleBrand>()
                .Include(b => b.VehicleTypeBrands)
                    .ThenInclude(vtb => vtb.VehicleType)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
        }
    }
}
