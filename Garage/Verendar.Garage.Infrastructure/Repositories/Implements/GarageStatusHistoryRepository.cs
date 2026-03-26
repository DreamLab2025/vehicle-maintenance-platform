namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageStatusHistoryRepository(GarageDbContext context)
    : PostgresRepository<GarageStatusHistory>(context), IGarageStatusHistoryRepository
{
}
