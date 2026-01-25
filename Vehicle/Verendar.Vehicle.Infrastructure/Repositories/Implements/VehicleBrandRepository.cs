using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleBrandRepository : PostgresRepository<VehicleBrand>, IVehicleBrandRepository
    {
        public VehicleBrandRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<VehicleBrand?> GetByIdWithTypesAsync(Guid id)
        {
            return await _context.Set<VehicleBrand>()
                .Include(b => b.VehicleType)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
        }
    }
}
