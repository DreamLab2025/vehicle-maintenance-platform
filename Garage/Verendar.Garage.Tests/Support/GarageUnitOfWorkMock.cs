using Moq;
using Verendar.Garage.Domain.Repositories.Interfaces;

namespace Verendar.Garage.Tests.Support;

internal sealed class GarageUnitOfWorkMock
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Loose);
    public Mock<IGarageRepository> Garages { get; } = new(MockBehavior.Strict);
    public Mock<IGarageBranchRepository> GarageBranches { get; } = new(MockBehavior.Strict);
    public Mock<IGarageMemberRepository> Members { get; } = new(MockBehavior.Strict);
    public Mock<IGarageStatusHistoryRepository> StatusHistories { get; } = new(MockBehavior.Strict);

    public GarageUnitOfWorkMock()
    {
        UnitOfWork.Setup(u => u.Garages).Returns(Garages.Object);
        UnitOfWork.Setup(u => u.GarageBranches).Returns(GarageBranches.Object);
        UnitOfWork.Setup(u => u.Members).Returns(Members.Object);
        UnitOfWork.Setup(u => u.StatusHistories).Returns(StatusHistories.Object);
        UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }
}
