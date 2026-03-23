namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageProductRepository(GarageDbContext context)
    : PostgresRepository<GarageProduct>(context), IGarageProductRepository
{
}
