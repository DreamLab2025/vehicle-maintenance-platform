namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageAccountRepository(GarageDbContext context)
    : PostgresRepository<GarageAccount>(context), IGarageAccountRepository
{
}
