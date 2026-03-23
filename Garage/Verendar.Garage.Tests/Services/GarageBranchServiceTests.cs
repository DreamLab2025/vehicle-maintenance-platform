using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.ExternalServices;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
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

        var geo = new Mock<IGeocodingService>(MockBehavior.Strict);

        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.CreateBranchAsync(garageId, ownerId, CreateValidRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(
            result,
            404,
            $"Không tìm thấy garage với id '{garageId}'.");
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        geo.Verify(g => g.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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
                Slug = "g"
            }
        };

        var m = new GarageUnitOfWorkMock();
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((Expression<Func<GarageEntity, bool>> expr) =>
                garages.FirstOrDefault(g => expr.Compile()(g)));

        var geo = new Mock<IGeocodingService>(MockBehavior.Strict);

        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.CreateBranchAsync(garageId, otherUser, CreateValidRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(
            result,
            403,
            "Bạn không có quyền thêm chi nhánh cho garage này.");
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        geo.Verify(g => g.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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
                Slug = "g"
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

        var geo = new Mock<IGeocodingService>(MockBehavior.Strict);
        geo.Setup(g => g.GeocodeAsync("123 Phố Huế, Việt Nam", It.IsAny<CancellationToken>()))
            .ReturnsAsync((10.762622, 106.660172));

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
                Slug = "g"
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

        var geo = new Mock<IGeocodingService>(MockBehavior.Strict);
        geo.Setup(g => g.GeocodeAsync("123 Phố Huế, Việt Nam", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((double, double)?)null);

        var sut = new GarageBranchService(NullLogger<GarageBranchService>.Instance, m.UnitOfWork.Object, geo.Object);

        var result = await sut.CreateBranchAsync(garageId, ownerId, CreateValidRequest());

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, "Tạo chi nhánh thành công");
        result.Data.Should().NotBeNull();
        result.Data!.Latitude.Should().Be(0);
        result.Data.Longitude.Should().Be(0);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
