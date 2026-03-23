namespace Verendar.Location.Tests.Support;

using Moq;
using Verendar.Location.Domain.Repositories.Interfaces;

internal sealed class LocationUnitOfWorkMock
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Loose);
    public Mock<IProvinceRepository> Provinces { get; } = new(MockBehavior.Strict);
    public Mock<IWardRepository> Wards { get; } = new(MockBehavior.Strict);
    public Mock<IAdministrativeUnitRepository> AdministrativeUnits { get; } = new(MockBehavior.Strict);
    public Mock<IAdministrativeRegionRepository> AdministrativeRegions { get; } = new(MockBehavior.Strict);

    public LocationUnitOfWorkMock()
    {
        UnitOfWork.Setup(u => u.Provinces).Returns(Provinces.Object);
        UnitOfWork.Setup(u => u.Wards).Returns(Wards.Object);
        UnitOfWork.Setup(u => u.AdministrativeUnits).Returns(AdministrativeUnits.Object);
        UnitOfWork.Setup(u => u.AdministrativeRegions).Returns(AdministrativeRegions.Object);
    }
}
