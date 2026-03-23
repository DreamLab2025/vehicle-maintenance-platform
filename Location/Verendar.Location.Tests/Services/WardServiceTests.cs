namespace Verendar.Location.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Caching;
using Verendar.Location.Application.Dtos;
using Verendar.Location.Application.Services.Implements;
using Verendar.Location.Application.Shared.Const;
using Verendar.Location.Domain.Entities;

public class WardServiceTests
{
    [Fact]
    public async Task GetWardByCodeAsync_WhenNotFound_ReturnsNotFound()
    {
        var m = new LocationUnitOfWorkMock();
        m.Wards.Setup(w => w.GetByCodeAsync("x")).ReturnsAsync((Ward?)null);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<WardResponse>(CacheKeys.WardByCode("x"))).ReturnsAsync((WardResponse?)null);

        var sut = new WardService(NullLogger<WardService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetWardByCodeAsync("x");

        LocationServiceResponseAssert.AssertFailureEnvelope(result, 404, "Phường/xã không tồn tại");
        cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<WardResponse>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact]
    public async Task GetWardByCodeAsync_WhenFound_SetsCacheAndReturnsMappedResponse()
    {
        var m = new LocationUnitOfWorkMock();
        var province = new Province { Code = "01", Name = "Hà Nội", AdministrativeRegionId = 3, AdministrativeUnitId = 1 };
        var ward = new Ward
        {
            Code = "00004",
            Name = "Ba Đình",
            ProvinceCode = "01",
            AdministrativeUnitId = 3,
            Province = province,
            AdministrativeUnit = new AdministrativeUnit { Id = 3, Name = "Phường", Abbreviation = "Phường" }
        };
        m.Wards.Setup(w => w.GetByCodeAsync("00004")).ReturnsAsync(ward);

        var key = CacheKeys.WardByCode("00004");
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<WardResponse>(key)).ReturnsAsync((WardResponse?)null);
        cache.Setup(c => c.SetAsync(key, It.IsAny<WardResponse>(), CacheKeys.DefaultCacheDuration))
            .Returns(Task.CompletedTask);

        var sut = new WardService(NullLogger<WardService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetWardByCodeAsync("00004");

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy thông tin phường/xã thành công");
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("00004");
        result.Data.Name.Should().Be("Ba Đình");
        result.Data.ProvinceCode.Should().Be("01");
        result.Data.ProvinceName.Should().Be("Hà Nội");
        result.Data.AdministrativeUnitId.Should().Be(3);
        result.Data.AdministrativeUnitName.Should().Be("Phường");
        cache.Verify(c => c.SetAsync(key, It.IsAny<WardResponse>(), CacheKeys.DefaultCacheDuration), Times.Once);
    }

    [Fact]
    public async Task GetWardByCodeAsync_WhenCacheHit_ReturnsWithoutRepository()
    {
        var m = new LocationUnitOfWorkMock();
        var cached = new WardResponse { Code = "00004", Name = "Ba Đình", ProvinceCode = "01" };
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<WardResponse>(CacheKeys.WardByCode("00004"))).ReturnsAsync(cached);

        var sut = new WardService(NullLogger<WardService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetWardByCodeAsync("00004");

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy thông tin phường/xã thành công");
        result.Data.Should().BeSameAs(cached);
        m.Wards.Verify(w => w.GetByCodeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetWardByCodeAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        var m = new LocationUnitOfWorkMock();
        m.Wards.Setup(w => w.GetByCodeAsync("00004")).ThrowsAsync(new InvalidOperationException("db"));

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<WardResponse>(CacheKeys.WardByCode("00004"))).ReturnsAsync((WardResponse?)null);

        var sut = new WardService(NullLogger<WardService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetWardByCodeAsync("00004");

        LocationServiceResponseAssert.AssertFailureEnvelope(result, 400, "Lỗi khi lấy thông tin phường/xã");
    }
}
