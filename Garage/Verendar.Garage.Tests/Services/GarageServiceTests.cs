using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
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

    [Fact]
    public async Task GetGaragesAsync_WithNoStatusFilter_ReturnsAllGaragesPaged()
    {
        var garages = new List<GarageEntity>
        {
            new() { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), BusinessName = "Garage A", Slug = "garage-a", Status = GarageStatus.Active },
            new() { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), BusinessName = "Garage B", Slug = "garage-b", Status = GarageStatus.Pending }
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<GarageEntity, bool>>>(),
                It.IsAny<Func<IQueryable<GarageEntity>, IOrderedQueryable<GarageEntity>>>()))
            .ReturnsAsync((garages, garages.Count));

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object);

        var result = await sut.GetGaragesAsync(new GarageFilterRequest { PageNumber = 1, PageSize = 10 });

        var meta = GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, "Lấy danh sách garage thành công", 2, 2);
        meta.PageNumber.Should().Be(1);
        meta.PageSize.Should().Be(10);
        result.Data![0].BusinessName.Should().Be("Garage A");
        result.Data[1].BusinessName.Should().Be("Garage B");
    }

    [Fact]
    public async Task GetGaragesAsync_WithStatusFilter_PassesFilterAndReturnsOnlyMatchingGarages()
    {
        var activeGarage = new GarageEntity
        {
            Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(),
            BusinessName = "Active Garage", Slug = "active-garage", Status = GarageStatus.Active
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsNotNull<Expression<Func<GarageEntity, bool>>>(),
                It.IsAny<Func<IQueryable<GarageEntity>, IOrderedQueryable<GarageEntity>>>()))
            .ReturnsAsync((new List<GarageEntity> { activeGarage }, 1));

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object);

        var result = await sut.GetGaragesAsync(new GarageFilterRequest { Status = GarageStatus.Active });

        GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, "Lấy danh sách garage thành công", 1, 1);
        result.Data![0].Status.Should().Be(GarageStatus.Active);
        m.Garages.Verify(r => r.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(),
            It.IsNotNull<Expression<Func<GarageEntity, bool>>>(),
            It.IsAny<Func<IQueryable<GarageEntity>, IOrderedQueryable<GarageEntity>>>()), Times.Once);
    }

    [Fact]
    public async Task GetGaragesAsync_WhenNoGaragesExist_Returns200WithEmptyListAndZeroTotal()
    {
        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<GarageEntity, bool>>>(),
                It.IsAny<Func<IQueryable<GarageEntity>, IOrderedQueryable<GarageEntity>>>()))
            .ReturnsAsync((new List<GarageEntity>(), 0));

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object);

        var result = await sut.GetGaragesAsync(new GarageFilterRequest());

        var meta = GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, "Lấy danh sách garage thành công", 0, 0);
        meta.TotalPages.Should().Be(0);
        meta.HasNextPage.Should().BeFalse();
        meta.HasPreviousPage.Should().BeFalse();
    }
}
