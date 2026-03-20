using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleModelRepository(VehicleDbContext context) : PostgresRepository<VehicleModel>(context), IVehicleModelRepository
    {
    }
}
