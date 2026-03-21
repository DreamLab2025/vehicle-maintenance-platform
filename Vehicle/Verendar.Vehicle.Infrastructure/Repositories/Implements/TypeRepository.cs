using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class TypeRepository(VehicleDbContext context) : PostgresRepository<VehicleType>(context), ITypeRepository
    {
    }
}
