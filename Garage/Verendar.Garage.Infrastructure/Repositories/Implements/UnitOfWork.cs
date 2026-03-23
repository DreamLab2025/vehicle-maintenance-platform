using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class UnitOfWork(GarageDbContext context)
    : BaseUnitOfWork<GarageDbContext>(context), IUnitOfWork
{
    private IGarageAccountRepository? _garageAccounts;
    private IGarageBranchRepository? _garageBranches;
    private IGarageProductRepository? _garageProducts;
    private IMechanicRepository? _mechanics;
    private IBookingRepository? _bookings;
    private IReviewRepository? _reviews;

    public IGarageAccountRepository GarageAccounts =>
        _garageAccounts ??= new GarageAccountRepository(Context);

    public IGarageBranchRepository GarageBranches =>
        _garageBranches ??= new GarageBranchRepository(Context);

    public IGarageProductRepository GarageProducts =>
        _garageProducts ??= new GarageProductRepository(Context);

    public IMechanicRepository Mechanics =>
        _mechanics ??= new MechanicRepository(Context);

    public IBookingRepository Bookings =>
        _bookings ??= new BookingRepository(Context);

    public IReviewRepository Reviews =>
        _reviews ??= new ReviewRepository(Context);
}
