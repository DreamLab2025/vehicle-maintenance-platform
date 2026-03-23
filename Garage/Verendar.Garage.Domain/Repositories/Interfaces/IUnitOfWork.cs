using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IGarageAccountRepository GarageAccounts { get; }
    IGarageBranchRepository GarageBranches { get; }
    IGarageProductRepository GarageProducts { get; }
    IMechanicRepository Mechanics { get; }
    IBookingRepository Bookings { get; }
    IReviewRepository Reviews { get; }
}
