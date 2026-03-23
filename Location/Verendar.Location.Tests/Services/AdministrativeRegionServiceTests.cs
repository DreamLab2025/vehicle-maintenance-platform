namespace Verendar.Location.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Caching;
using Verendar.Location.Application.Dtos;
using Verendar.Location.Application.Services.Implements;
using Verendar.Location.Application.Shared.Const;
using Verendar.Location.Domain.Entities;

public class AdministrativeRegionServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenCacheHit_ReturnsCached()
    {
        var m = new LocationUnitOfWorkMock();
        var cached = new List<AdministrativeRegionResponse> { new() { Id = 1, Name = "Đông Bắc Bộ" } };
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<AdministrativeRegionResponse>>(CacheKeys.AdministrativeRegionsAll))
            .ReturnsAsync(cached);

        var sut = new AdministrativeRegionService(
            NullLogger<AdministrativeRegionService>.Instance,
            m.UnitOfWork.Object,
            cache.Object);

        var result = await sut.GetAllAsync();

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách vùng miền thành công");
        result.Data.Should().BeSameAs(cached);
        m.AdministrativeRegions.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WhenCacheMiss_LoadsAndCaches()
    {
        var m = new LocationUnitOfWorkMock();
        var entities = new List<AdministrativeRegion>
        {
            new() { Id = 1, Name = "Đông Bắc Bộ" },
            new() { Id = 2, Name = "Tây Bắc Bộ" }
        };
        m.AdministrativeRegions.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<AdministrativeRegionResponse>>(CacheKeys.AdministrativeRegionsAll))
            .ReturnsAsync((List<AdministrativeRegionResponse>?)null);
        cache.Setup(c => c.SetAsync(CacheKeys.AdministrativeRegionsAll, It.IsAny<List<AdministrativeRegionResponse>>(), CacheKeys.DefaultCacheDuration))
            .Returns(Task.CompletedTask);

        var sut = new AdministrativeRegionService(
            NullLogger<AdministrativeRegionService>.Instance,
            m.UnitOfWork.Object,
            cache.Object);

        var result = await sut.GetAllAsync();

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách vùng miền thành công");
        result.Data.Should().NotBeNull().And.HaveCount(2);
        result.Data![0].Id.Should().Be(1);
        result.Data[0].Name.Should().Be("Đông Bắc Bộ");
        result.Data[1].Id.Should().Be(2);
        result.Data[1].Name.Should().Be("Tây Bắc Bộ");
        m.AdministrativeRegions.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        var m = new LocationUnitOfWorkMock();
        m.AdministrativeRegions.Setup(r => r.GetAllAsync()).ThrowsAsync(new InvalidOperationException("db"));

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<AdministrativeRegionResponse>>(CacheKeys.AdministrativeRegionsAll))
            .ReturnsAsync((List<AdministrativeRegionResponse>?)null);

        var sut = new AdministrativeRegionService(
            NullLogger<AdministrativeRegionService>.Instance,
            m.UnitOfWork.Object,
            cache.Object);

        var result = await sut.GetAllAsync();

        LocationServiceResponseAssert.AssertFailureEnvelope(result, 400, "Lỗi khi lấy danh sách vùng miền");
    }
}
