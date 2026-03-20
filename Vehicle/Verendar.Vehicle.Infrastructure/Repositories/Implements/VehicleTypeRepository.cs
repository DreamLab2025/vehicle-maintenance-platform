using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleTypeRepository(VehicleDbContext context) : PostgresRepository<VehicleType>(context), IVehicleTypeRepository
    {
    }
}
