using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Tests.Services;

public class GarageServiceTests
{
    [Fact]
    public async Task CreateGarageAsync_WhenOwnerAlreadyHasGarage_Returns409()
    {
        var ownerId = Guid.NewGuid();
        var existing = new GarageEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            BusinessName = "Existing",
            Slug = "existing"
        };
        var store = new List<GarageEntity> { existing };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) =>
                store.FirstOrDefault(g => expr.Compile()(g)));

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object);

        var result = await sut.CreateGarageAsync(ownerId, new GarageRequest { BusinessName = "New Name" });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 409, "Tài khoản đã có garage đăng ký");
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        m.Garages.Verify(r => r.AddAsync(It.IsAny<GarageEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateGarageAsync_WhenNewOwner_CreatesGarageAndReturns201()
    {
        var ownerId = Guid.NewGuid();
        var store = new List<GarageEntity>();

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) =>
                store.FirstOrDefault(g => expr.Compile()(g)));
        m.Garages.Setup(r => r.AddAsync(It.IsAny<GarageEntity>()))
            .ReturnsAsync((GarageEntity g) => g);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object);

        var request = new GarageRequest { BusinessName = "Garage Alpha" };
        var result = await sut.CreateGarageAsync(ownerId, request);

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, "Đăng ký garage thành công");
        result.Data.Should().NotBeNull();
        result.Data!.OwnerId.Should().Be(ownerId);
        result.Data.BusinessName.Should().Be("Garage Alpha");
        result.Data.Slug.Should().NotBeNullOrWhiteSpace();
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        m.Garages.Verify(r => r.AddAsync(It.IsAny<GarageEntity>()), Times.Once);
    }
}
