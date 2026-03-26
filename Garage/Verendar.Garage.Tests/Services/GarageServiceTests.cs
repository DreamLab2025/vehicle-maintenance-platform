using System.Linq.Expressions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Contracts.Events;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using Verendar.Garage.Domain.ValueObjects;
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

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.CreateGarageAsync(ownerId, new GarageRequest { BusinessName = "New Name" });

        GarageServiceResponseAssert.AssertFailureEnvelope(
            result,
            409,
            "Tài khoản đã có garage đăng ký. Nếu bị từ chối, hãy chỉnh sửa và nộp lại.");
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

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

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

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

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

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

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

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.GetGaragesAsync(new GarageFilterRequest());

        var meta = GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, "Lấy danh sách garage thành công", 0, 0);
        meta.TotalPages.Should().Be(0);
        meta.HasNextPage.Should().BeFalse();
        meta.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetMyGarageAsync_WhenOwnerHasGarage_Returns200WithBranches()
    {
        var ownerId = Guid.NewGuid();
        var garage = new GarageEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            BusinessName = "My Garage",
            Slug = "my-garage",
            Branches =
            [
                new GarageBranch { Id = Guid.NewGuid(), Name = "Branch A", Slug = "branch-a", Address = new Address { ProvinceCode = "79", WardCode = "00001", StreetDetail = "Street A" } },
                new GarageBranch { Id = Guid.NewGuid(), Name = "Branch B", Slug = "branch-b", Address = new Address { ProvinceCode = "79", WardCode = "00002", StreetDetail = "Street B" } }
            ]
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.GetWithBranchesAsync(
                It.IsAny<Expression<Func<GarageEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr, CancellationToken _) =>
                expr.Compile()(garage) ? garage : null);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.GetMyGarageAsync(ownerId);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy thông tin garage thành công");
        result.Data!.BusinessName.Should().Be("My Garage");
        result.Data.BranchCount.Should().Be(2);
        result.Data.Branches.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMyGarageAsync_WhenOwnerHasNoGarage_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.GetWithBranchesAsync(
                It.IsAny<Expression<Func<GarageEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GarageEntity?)null);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.GetMyGarageAsync(Guid.NewGuid());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, "Bạn chưa đăng ký garage.");
    }

    [Fact]
    public async Task GetGarageByIdAsync_WhenGarageExists_Returns200WithBranches()
    {
        var garageId = Guid.NewGuid();
        var garage = new GarageEntity
        {
            Id = garageId,
            OwnerId = Guid.NewGuid(),
            BusinessName = "Target Garage",
            Slug = "target-garage",
            Branches =
            [
                new GarageBranch { Id = Guid.NewGuid(), Name = "Main Branch", Slug = "main-branch", Address = new Address { ProvinceCode = "79", WardCode = "00001", StreetDetail = "Main Street" } }
            ]
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.GetWithBranchesAsync(
                It.IsAny<Expression<Func<GarageEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr, CancellationToken _) =>
                expr.Compile()(garage) ? garage : null);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.GetGarageByIdAsync(garageId);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy thông tin garage thành công");
        result.Data!.Id.Should().Be(garageId);
        result.Data.BusinessName.Should().Be("Target Garage");
        result.Data.BranchCount.Should().Be(1);
        result.Data.Branches.Should().HaveCount(1);
        result.Data.Branches[0].Name.Should().Be("Main Branch");
    }

    [Fact]
    public async Task GetGarageByIdAsync_WhenGarageNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.GetWithBranchesAsync(
                It.IsAny<Expression<Func<GarageEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GarageEntity?)null);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.GetGarageByIdAsync(Guid.NewGuid());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, "Không tìm thấy garage.");
    }

    [Fact]
    public async Task UpdateGarageStatusAsync_WhenGarageNotFound_Returns404()
    {
        var garageId = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((GarageEntity?)null);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.UpdateGarageStatusAsync(garageId, new UpdateGarageStatusRequest
        {
            Status = GarageStatus.Active
        }, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be($"Không tìm thấy garage với id '{garageId}'.");
    }

    [Fact]
    public async Task UpdateGarageStatusAsync_WhenInvalidTransition_Returns400()
    {
        var garageId = Guid.NewGuid();
        var garage = new GarageEntity
        {
            Id = garageId,
            OwnerId = Guid.NewGuid(),
            BusinessName = "G",
            Slug = "g",
            Status = GarageStatus.Active
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.UpdateGarageStatusAsync(garageId, new UpdateGarageStatusRequest
        {
            Status = GarageStatus.Pending
        }, Guid.NewGuid());

        GarageServiceResponseAssert.AssertFailureEnvelope(
            result,
            400,
            "Không thể chuyển trạng thái từ 'Active' sang 'Pending'.");
    }

    [Fact]
    public async Task UpdateGarageInfoAsync_WhenGarageOwnedAndEditable_Returns200()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var garage = new GarageEntity
        {
            Id = garageId,
            OwnerId = ownerId,
            BusinessName = "Old",
            Slug = "old",
            Status = GarageStatus.Pending
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.UpdateGarageInfoAsync(garageId, ownerId, new GarageRequest
        {
            BusinessName = "New Name",
            ShortName = "NN"
        });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Cập nhật thông tin garage thành công");
        result.Data!.BusinessName.Should().Be("New Name");
    }

    [Fact]
    public async Task ResubmitGarageAsync_WhenStatusIsRejected_Returns200()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var garage = new GarageEntity
        {
            Id = garageId,
            OwnerId = ownerId,
            BusinessName = "G",
            Slug = "g",
            Status = GarageStatus.Rejected
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.StatusHistories.Setup(r => r.AddAsync(It.IsAny<GarageStatusHistory>()))
            .ReturnsAsync((GarageStatusHistory h) => h);

        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.ResubmitGarageAsync(garageId, ownerId);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Nộp lại hồ sơ thành công");
        result.Data!.Status.Should().Be(GarageStatus.Pending);
    }

    [Fact]
    public async Task UpdateGarageStatusAsync_WhenValidTransition_Returns200AndPublishesEvent()
    {
        var garageId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var garage = new GarageEntity
        {
            Id = garageId,
            OwnerId = Guid.NewGuid(),
            BusinessName = "G",
            Slug = "g",
            Status = GarageStatus.Pending
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.StatusHistories.Setup(r => r.AddAsync(It.IsAny<GarageStatusHistory>()))
            .ReturnsAsync((GarageStatusHistory h) => h);

        var publish = new Mock<IPublishEndpoint>(MockBehavior.Strict);
        publish.Setup(p => p.Publish(It.IsAny<GarageStatusChangedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, publish.Object);

        var result = await sut.UpdateGarageStatusAsync(garageId, new UpdateGarageStatusRequest { Status = GarageStatus.Active }, adminId);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Cập nhật trạng thái garage thành công");
        garage.Status.Should().Be(GarageStatus.Active);
        publish.VerifyAll();
    }

    [Fact]
    public async Task UpdateGarageInfoAsync_WhenStatusActive_Returns400()
    {
        var garage = new GarageEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            BusinessName = "G",
            Slug = "g",
            Status = GarageStatus.Active
        };
        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object, Mock.Of<IPublishEndpoint>());

        var result = await sut.UpdateGarageInfoAsync(garage.Id, garage.OwnerId, new GarageRequest { BusinessName = "N" });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 400, "Không thể chỉnh sửa thông tin garage khi đang ở trạng thái Active hoặc Suspended.");
    }
}
