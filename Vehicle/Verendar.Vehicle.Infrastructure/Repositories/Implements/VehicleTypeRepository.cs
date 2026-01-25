using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleTypeRepository(VehicleDbContext context) : PostgresRepository<VehicleType>(context), IVehicleTypeRepository
    {
    }
}
