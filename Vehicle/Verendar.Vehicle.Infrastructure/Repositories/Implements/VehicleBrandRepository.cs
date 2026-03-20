using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleBrandRepository(VehicleDbContext context) : PostgresRepository<VehicleBrand>(context), IVehicleBrandRepository
    {
        public async Task<VehicleBrand?> GetByIdWithTypesAsync(Guid id)
        {
            return await _context.Set<VehicleBrand>()
                .Include(b => b.VehicleType)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
        }
    }
}
