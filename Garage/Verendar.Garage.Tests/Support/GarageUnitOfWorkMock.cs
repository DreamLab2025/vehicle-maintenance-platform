using Moq;
using Verendar.Garage.Domain.Repositories.Interfaces;

namespace Verendar.Garage.Tests.Support;

internal sealed class GarageUnitOfWorkMock
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Loose);
    public Mock<IGarageRepository> Garages { get; } = new(MockBehavior.Strict);
    public Mock<IGarageBranchRepository> GarageBranches { get; } = new(MockBehavior.Strict);
    public Mock<IGarageProductRepository> GarageProducts { get; } = new(MockBehavior.Strict);
    public Mock<IGarageServiceRepository> GarageServices { get; } = new(MockBehavior.Strict);
    public Mock<IGarageBundleRepository> GarageBundles { get; } = new(MockBehavior.Strict);
    public Mock<IServiceCategoryRepository> ServiceCategories { get; } = new(MockBehavior.Strict);
    public Mock<IGarageMemberRepository> Members { get; } = new(MockBehavior.Strict);
    public Mock<IBookingRepository> Bookings { get; } = new(MockBehavior.Strict);
    public Mock<IReviewRepository> Reviews { get; } = new(MockBehavior.Strict);
    public Mock<IGarageStatusHistoryRepository> StatusHistories { get; } = new(MockBehavior.Strict);

    public GarageUnitOfWorkMock()
    {
        UnitOfWork.Setup(u => u.Garages).Returns(Garages.Object);
        UnitOfWork.Setup(u => u.GarageBranches).Returns(GarageBranches.Object);
        UnitOfWork.Setup(u => u.GarageProducts).Returns(GarageProducts.Object);
        UnitOfWork.Setup(u => u.GarageServices).Returns(GarageServices.Object);
        UnitOfWork.Setup(u => u.GarageBundles).Returns(GarageBundles.Object);
        UnitOfWork.Setup(u => u.ServiceCategories).Returns(ServiceCategories.Object);
        UnitOfWork.Setup(u => u.Members).Returns(Members.Object);
        UnitOfWork.Setup(u => u.Bookings).Returns(Bookings.Object);
        UnitOfWork.Setup(u => u.Reviews).Returns(Reviews.Object);
        UnitOfWork.Setup(u => u.StatusHistories).Returns(StatusHistories.Object);
        UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        Reviews.Setup(r => r.GetRatingSummaryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((0d, 0));
        Reviews.Setup(r => r.GetBulkRatingSummaryAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, (double AverageRating, int ReviewCount)>());
    }
}
