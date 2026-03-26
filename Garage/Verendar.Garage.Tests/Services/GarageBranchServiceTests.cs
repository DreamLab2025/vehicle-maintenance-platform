using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Tests.Services;

public class GarageBranchServiceTests
{
    private static GarageBranchRequest CreateValidRequest() => new()
    {
        Name = "Chi nhánh 1",
        Address = new AddressDto
        {
            ProvinceCode = "01",
            WardCode = "00001",
            StreetDetail = "123 Phố Huế"
        },
        WorkingHours = new WorkingHoursDto
        {
            Schedule = new Dictionary<DayOfWeek, DayScheduleDto>
            {
                [DayOfWeek.Monday] = new()
                {
                    OpenTime = new TimeOnly(8, 0),
                    CloseTime = new TimeOnly(18, 0),
                    IsClosed = false
                }
            }
        }
    };

    [Fact]
    public async Task CreateBranchAsync_WhenGarageNotFound_Returns404()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var garages = new List<GarageEntity>();

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) =>
                garages.FirstOrDefault(g => expr.Compile()(g)));

        var geo = new Mock<ILocationClient>(MockBehavior.Strict);

        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.CreateBranchAsync(garageId, ownerId, CreateValidRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(
            result,
            404,
            $"Không tìm thấy garage với id '{garageId}'.");
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        geo.Verify(l => l.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBranchAsync_WhenRequesterIsNotOwner_Returns403()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUser = Guid.NewGuid();
        var garages = new List<GarageEntity>
        {
            new()
            {
                Id = garageId,
                OwnerId = ownerId,
                BusinessName = "G",
                Slug = "g",
                Status = GarageStatus.Active
            }
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) =>
                garages.FirstOrDefault(g => expr.Compile()(g)));

        var geo = new Mock<ILocationClient>(MockBehavior.Strict);

        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.CreateBranchAsync(garageId, otherUser, CreateValidRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(
            result,
            403,
            "Bạn không có quyền thêm chi nhánh cho garage này.");
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        geo.Verify(l => l.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBranchAsync_WhenGeocodingSucceeds_PersistsCoordinates()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var garages = new List<GarageEntity>
        {
            new()
            {
                Id = garageId,
                OwnerId = ownerId,
                BusinessName = "G",
                Slug = "g",
                Status = GarageStatus.Active
            }
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) =>
                garages.FirstOrDefault(g => expr.Compile()(g)));
        var branchStore = new List<GarageBranch>();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageBranch, bool>> expr) =>
                branchStore.FirstOrDefault(b => expr.Compile()(b)));
        m.GarageBranches.Setup(r => r.AddAsync(It.IsAny<GarageBranch>()))
            .ReturnsAsync((GarageBranch b) => b);

        var geo = new Mock<ILocationClient>(MockBehavior.Strict);
        geo.Setup(l => l.ValidateLocationAsync("01", "00001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Hà Nội", "Phường Trúc Bạch"));
        geo.Setup(l => l.GeocodeAsync("123 Phố Huế, Phường Trúc Bạch, Hà Nội", It.IsAny<CancellationToken>()))
            .ReturnsAsync((10.762622, 106.660172));
        geo.Setup(l => l.GetMapLinksAsync(10.762622, 106.660172, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MapLinksDto?)null);

        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.CreateBranchAsync(garageId, ownerId, CreateValidRequest());

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, "Tạo chi nhánh thành công");
        result.Data.Should().NotBeNull();
        result.Data!.Latitude.Should().Be(10.762622);
        result.Data.Longitude.Should().Be(106.660172);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBranchAsync_WhenGeocodingFails_StillCreatesBranchWithZeroCoordinates()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var garages = new List<GarageEntity>
        {
            new()
            {
                Id = garageId,
                OwnerId = ownerId,
                BusinessName = "G",
                Slug = "g",
                Status = GarageStatus.Active
            }
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) =>
                garages.FirstOrDefault(g => expr.Compile()(g)));
        var branchStore = new List<GarageBranch>();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageBranch, bool>> expr) =>
                branchStore.FirstOrDefault(b => expr.Compile()(b)));
        m.GarageBranches.Setup(r => r.AddAsync(It.IsAny<GarageBranch>()))
            .ReturnsAsync((GarageBranch b) => b);

        var geo = new Mock<ILocationClient>(MockBehavior.Strict);
        geo.Setup(l => l.ValidateLocationAsync("01", "00001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Hà Nội", "Phường Trúc Bạch"));
        geo.Setup(l => l.GeocodeAsync("123 Phố Huế, Phường Trúc Bạch, Hà Nội", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((double, double)?)null);

        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.CreateBranchAsync(garageId, ownerId, CreateValidRequest());

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, "Tạo chi nhánh thành công");
        result.Data.Should().NotBeNull();
        result.Data!.Latitude.Should().Be(0);
        result.Data.Longitude.Should().Be(0);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBranchesAsync_WhenGarageExists_ReturnsPagedData()
    {
        var garageId = Guid.NewGuid();
        var garages = new List<GarageEntity> { new() { Id = garageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g" } };
        var branch = new GarageBranch
        {
            Id = Guid.NewGuid(),
            GarageId = garageId,
            Name = "B",
            Slug = "b",
            Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" },
            WorkingHours = new() { Schedule = [] }
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) => garages.FirstOrDefault(g => expr.Compile()(g)));
        m.GarageBranches.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<GarageBranch, bool>>>(),
                It.IsAny<Func<IQueryable<GarageBranch>, IOrderedQueryable<GarageBranch>>>()))
            .ReturnsAsync((new List<GarageBranch> { branch }, 1));

        var geo = new Mock<ILocationClient>(MockBehavior.Strict);
        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.GetBranchesAsync(garageId, new PaginationRequest(), CancellationToken.None);

        GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, "Lấy danh sách chi nhánh thành công", 1, 1);
    }

    [Fact]
    public async Task DeleteBranchAsync_WhenOwnerDeletes_Returns200()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var garage = new GarageEntity { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var branch = new GarageBranch
        {
            Id = branchId,
            GarageId = garageId,
            Name = "B",
            Slug = "b",
            Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" },
            WorkingHours = new() { Schedule = [] }
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);

        var geo = new Mock<ILocationClient>(MockBehavior.Strict);
        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.DeleteBranchAsync(garageId, branchId, ownerId, CancellationToken.None);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Xóa chi nhánh thành công");
        branch.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBranchByIdAsync_WhenHasCoordinates_ReturnsMapLinks()
    {
        var garageId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var branch = new GarageBranch
        {
            Id = branchId,
            GarageId = garageId,
            Name = "B",
            Slug = "b",
            Latitude = 10,
            Longitude = 106,
            Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" },
            WorkingHours = new() { Schedule = [] }
        };

        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);

        var geo = new Mock<ILocationClient>(MockBehavior.Strict);
        geo.Setup(l => l.GetMapLinksAsync(10, 106, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MapLinksDto { GoogleMaps = "g", AppleMaps = "a", Waze = "w", OpenStreetMap = "o" });
        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.GetBranchByIdAsync(garageId, branchId, CancellationToken.None);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy thông tin chi nhánh thành công");
        result.Data!.MapLinks.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateBranchStatusAsync_WhenOwnerAndGarageActive_Returns200()
    {
        var garageId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var garage = new GarageEntity { Id = garageId, OwnerId = ownerId, BusinessName = "G", Slug = "g", Status = GarageStatus.Active };
        var branch = new GarageBranch
        {
            Id = branchId,
            GarageId = garageId,
            Name = "B",
            Slug = "b",
            Latitude = 10,
            Longitude = 106,
            Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" },
            WorkingHours = new() { Schedule = [] }
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        var geo = new Mock<ILocationClient>(MockBehavior.Strict);
        geo.Setup(l => l.GetMapLinksAsync(10, 106, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MapLinksDto?)null);
        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.UpdateBranchStatusAsync(garageId, branchId, ownerId, new UpdateBranchStatusRequest { Status = BranchStatus.Inactive }, CancellationToken.None);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Cập nhật trạng thái chi nhánh thành công");
        branch.Status.Should().Be(BranchStatus.Inactive);
    }

    [Fact]
    public async Task GetBranchesForMapAsync_WhenAddressGeocodeFails_ReturnsEmptyPaged()
    {
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.GetBranchesForMapAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<GarageBranch>(), 0));
        var geo = new Mock<ILocationClient>(MockBehavior.Strict);
        geo.Setup(l => l.GeocodeAsync("abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((double, double)?)null);
        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.GetBranchesForMapAsync(new BranchMapSearchRequest { Address = "abc" }, CancellationToken.None);

        GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, "Không tìm thấy địa chỉ", 0, 0);
    }

    [Fact]
    public async Task GetBranchesForMapAsync_WhenCoordinatesProvided_ReturnsPagedBranches()
    {
        var garageId = Guid.NewGuid();
        var branch = new GarageBranch
        {
            Id = Guid.NewGuid(),
            GarageId = garageId,
            Name = "B",
            Slug = "b",
            Address = new() { ProvinceCode = "79", WardCode = "1", StreetDetail = "x" },
            WorkingHours = new() { Schedule = [] },
            Garage = new GarageEntity { Id = garageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g", Status = GarageStatus.Active }
        };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.GetBranchesForMapAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<double?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<GarageBranch> { branch }, 1));
        var geo = new Mock<ILocationClient>(MockBehavior.Strict);
        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.GetBranchesForMapAsync(new BranchMapSearchRequest { Lat = 10, Lng = 106, RadiusKm = 10 }, CancellationToken.None);

        GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, "Lấy danh sách chi nhánh thành công", 1, 1);
    }
}
