using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Garage.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IGarageRepository Garages { get; }
    IGarageBranchRepository GarageBranches { get; }
    IGarageProductRepository GarageProducts { get; }
    IGarageMemberRepository Members { get; }
    IBookingRepository Bookings { get; }
    IReviewRepository Reviews { get; }
    IGarageStatusHistoryRepository StatusHistories { get; }
}
