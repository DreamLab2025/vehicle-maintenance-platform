using Microsoft.EntityFrameworkCore;
using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
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
