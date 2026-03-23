namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageBranchRepository(GarageDbContext context)
    : PostgresRepository<GarageBranch>(context), IGarageBranchRepository
{
}
