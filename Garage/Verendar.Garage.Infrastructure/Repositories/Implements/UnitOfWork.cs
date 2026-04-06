namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class UnitOfWork(GarageDbContext context)
    : BaseUnitOfWork<GarageDbContext>(context), IUnitOfWork
{
    private IGarageRepository? _garages;
    private IGarageBranchRepository? _garageBranches;
    private IGarageProductRepository? _garageProducts;
    private IGarageServiceRepository? _garageServices;
    private IGarageBundleRepository? _garageBundles;
    private IServiceCategoryRepository? _serviceCategories;
    private IGarageMemberRepository? _members;
    private IBookingRepository? _bookings;
    private IReviewRepository? _reviews;
    private IGarageStatusHistoryRepository? _statusHistories;
    private IGarageReferralRepository? _referrals;

    public IGarageRepository Garages =>
        _garages ??= new GarageRepository(Context);

    public IGarageBranchRepository GarageBranches =>
        _garageBranches ??= new GarageBranchRepository(Context);

    public IGarageProductRepository GarageProducts =>
        _garageProducts ??= new GarageProductRepository(Context);

    public IGarageServiceRepository GarageServices =>
        _garageServices ??= new GarageServiceRepository(Context);

    public IGarageBundleRepository GarageBundles =>
        _garageBundles ??= new GarageBundleRepository(Context);

    public IServiceCategoryRepository ServiceCategories =>
        _serviceCategories ??= new ServiceCategoryRepository(Context);

    public IGarageMemberRepository Members =>
        _members ??= new GarageMemberRepository(Context);

    public IBookingRepository Bookings =>
        _bookings ??= new BookingRepository(Context);

    public IReviewRepository Reviews =>
        _reviews ??= new ReviewRepository(Context);

    public IGarageStatusHistoryRepository StatusHistories =>
        _statusHistories ??= new GarageStatusHistoryRepository(Context);

    public IGarageReferralRepository Referrals =>
        _referrals ??= new GarageReferralRepository(Context);
}
