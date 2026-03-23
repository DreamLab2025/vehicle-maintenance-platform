using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class UnitOfWork(GarageDbContext context)
    : BaseUnitOfWork<GarageDbContext>(context), IUnitOfWork
{
    private IGarageRepository? _garages;
    private IGarageBranchRepository? _garageBranches;
    private IGarageProductRepository? _garageProducts;
    private IMechanicRepository? _mechanics;
    private IBookingRepository? _bookings;
    private IReviewRepository? _reviews;

    public IGarageRepository Garages =>
        _garages ??= new GarageRepository(Context);

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
