using GarageEntity = global::Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageRepository(GarageDbContext context)
    : PostgresRepository<GarageEntity>(context), IGarageRepository
{
}
