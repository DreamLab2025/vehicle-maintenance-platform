namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class UnitOfWork(GarageDbContext context)
    : BaseUnitOfWork<GarageDbContext>(context), IUnitOfWork
{
}
