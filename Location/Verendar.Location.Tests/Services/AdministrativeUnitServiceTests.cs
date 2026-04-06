namespace Verendar.Location.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Caching;
using Verendar.Location.Application.Dtos;
using Verendar.Location.Application.Services.Implements;
using Verendar.Location.Application.Shared.Const;
using Verendar.Location.Domain.Entities;

public class AdministrativeUnitServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenCacheHit_ReturnsCached()
    {
        var m = new LocationUnitOfWorkMock();
        var cached = new List<AdministrativeUnitResponse>
        {
            new() { Id = 1, Name = "Thành phố trực thuộc trung ương", Abbreviation = "Thành phố" }
        };
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<AdministrativeUnitResponse>>(CacheKeys.AdministrativeUnitsAll))
            .ReturnsAsync(cached);

        var sut = new AdministrativeUnitService(
            NullLogger<AdministrativeUnitService>.Instance,
            m.UnitOfWork.Object,
            cache.Object);

        var result = await sut.GetAllAsync();

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách loại đơn vị thành công");
        result.Data.Should().BeSameAs(cached);
        m.AdministrativeUnits.Verify(u => u.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WhenCacheMiss_LoadsAndCaches()
    {
        var m = new LocationUnitOfWorkMock();
        var entities = new List<AdministrativeUnit>
        {
            new() { Id = 1, Name = "Thành phố trực thuộc trung ương", Abbreviation = "Thành phố" },
            new() { Id = 2, Name = "Tỉnh", Abbreviation = "Tỉnh" }
        };
        m.AdministrativeUnits.Setup(u => u.GetAllAsync()).ReturnsAsync(entities);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<AdministrativeUnitResponse>>(CacheKeys.AdministrativeUnitsAll))
            .ReturnsAsync((List<AdministrativeUnitResponse>?)null);
        cache.Setup(c => c.SetAsync(CacheKeys.AdministrativeUnitsAll, It.IsAny<List<AdministrativeUnitResponse>>(), CacheKeys.DefaultCacheDuration))
            .Returns(Task.CompletedTask);

        var sut = new AdministrativeUnitService(
            NullLogger<AdministrativeUnitService>.Instance,
            m.UnitOfWork.Object,
            cache.Object);

        var result = await sut.GetAllAsync();

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách loại đơn vị thành công");
        result.Data.Should().NotBeNull().And.HaveCount(2);
        result.Data![0].Id.Should().Be(1);
        result.Data[0].Name.Should().Be("Thành phố trực thuộc trung ương");
        result.Data[0].Abbreviation.Should().Be("Thành phố");
        result.Data[1].Id.Should().Be(2);
        result.Data[1].Name.Should().Be("Tỉnh");
        result.Data[1].Abbreviation.Should().Be("Tỉnh");
        m.AdministrativeUnits.Verify(u => u.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        var m = new LocationUnitOfWorkMock();
        m.AdministrativeUnits.Setup(u => u.GetAllAsync()).ThrowsAsync(new InvalidOperationException("db"));

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<AdministrativeUnitResponse>>(CacheKeys.AdministrativeUnitsAll))
            .ReturnsAsync((List<AdministrativeUnitResponse>?)null);

        var sut = new AdministrativeUnitService(
            NullLogger<AdministrativeUnitService>.Instance,
            m.UnitOfWork.Object,
            cache.Object);

        var result = await sut.GetAllAsync();

        LocationServiceResponseAssert.AssertFailureEnvelope(result, 400, "Lỗi khi lấy danh sách loại đơn vị");
    }
}
