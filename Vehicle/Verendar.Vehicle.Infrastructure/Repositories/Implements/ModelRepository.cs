using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class ModelRepository(VehicleDbContext context) : PostgresRepository<Model>(context), IModelRepository
    {
    }
}
