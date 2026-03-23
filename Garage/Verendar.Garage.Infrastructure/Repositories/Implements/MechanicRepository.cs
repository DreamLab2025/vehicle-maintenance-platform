namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class MechanicRepository(GarageDbContext context)
    : PostgresRepository<Mechanic>(context), IMechanicRepository
{
}
