namespace Verendar.Location.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Caching;
using Verendar.Location.Application.Dtos;
using Verendar.Location.Application.Services.Implements;
using Verendar.Location.Application.Shared.Const;
using Verendar.Location.Domain.Entities;

public class ProvinceServiceTests
{
    [Fact]
    public async Task GetAllProvincesAsync_WhenCacheHit_ReturnsCachedAndSkipsRepository()
    {
        var m = new LocationUnitOfWorkMock();
        var cached = new List<ProvinceResponse>
        {
            new() { Code = "01", Name = "Hà Nội", AdministrativeRegionId = 3 }
        };
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll))
            .ReturnsAsync(cached);

        var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetAllProvincesAsync();

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách tỉnh thành công");
        result.Data.Should().BeSameAs(cached);
        result.Data.Should().NotBeNull();
        result.Data![0].AdministrativeRegionName.Should().BeEmpty();
        result.Data![0].AdministrativeUnitName.Should().BeEmpty();
        m.Provinces.Verify(p => p.GetAllAsync(), Times.Never);
        cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<ProvinceResponse>>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact]
    public async Task GetAllProvincesAsync_WhenCacheMiss_LoadsFromRepositoryAndSetsCache()
    {
        var m = new LocationUnitOfWorkMock();
        var entities = new List<Province>
        {
            new()
            {
                Code = "01",
                Name = "Hà Nội",
                AdministrativeRegionId = 3,
                AdministrativeUnitId = 1,
                AdministrativeRegion = new AdministrativeRegion { Id = 3, Name = "Đồng bằng sông Hồng" },
                AdministrativeUnit = new AdministrativeUnit { Id = 1, Name = "Thành phố trực thuộc trung ương", Abbreviation = "Thành phố" }
            }
        };
        m.Provinces.Setup(p => p.GetAllAsync()).ReturnsAsync(entities);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll))
            .ReturnsAsync((List<ProvinceResponse>?)null);
        cache.Setup(c => c.SetAsync(CacheKeys.ProvincesAll, It.IsAny<List<ProvinceResponse>>(), CacheKeys.DefaultCacheDuration))
            .Returns(Task.CompletedTask);

        var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetAllProvincesAsync();

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách tỉnh thành công");
        result.Data.Should().NotBeNull().And.HaveCount(1);
        var p = result.Data![0];
        p.Code.Should().Be("01");
        p.Name.Should().Be("Hà Nội");
        p.AdministrativeRegionId.Should().Be(3);
        p.AdministrativeRegionName.Should().Be("Đồng bằng sông Hồng");
        p.AdministrativeUnitId.Should().Be(1);
        p.AdministrativeUnitName.Should().Be("Thành phố trực thuộc trung ương");
        m.Provinces.Verify(pr => pr.GetAllAsync(), Times.Once);
        cache.Verify(
            c => c.SetAsync(CacheKeys.ProvincesAll, It.IsAny<List<ProvinceResponse>>(), CacheKeys.DefaultCacheDuration),
            Times.Once);
    }

    [Fact]
    public async Task GetAllProvincesAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        var m = new LocationUnitOfWorkMock();
        m.Provinces.Setup(p => p.GetAllAsync()).ThrowsAsync(new InvalidOperationException("db"));

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll))
            .ReturnsAsync((List<ProvinceResponse>?)null);

        var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetAllProvincesAsync();

        LocationServiceResponseAssert.AssertFailureEnvelope(result, 400, "Lỗi khi lấy danh sách tỉnh");
    }

    [Fact]
    public async Task GetProvinceByCodeAsync_WhenNotFound_ReturnsNotFound()
    {
        var m = new LocationUnitOfWorkMock();
        m.Provinces.Setup(p => p.GetByCodeAsync("99")).ReturnsAsync((Province?)null);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<ProvinceResponse>(CacheKeys.ProvinceByCode("99")))
            .ReturnsAsync((ProvinceResponse?)null);

        var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetProvinceByCodeAsync("99");

        LocationServiceResponseAssert.AssertFailureEnvelope(result, 404, "Tỉnh không tồn tại");
        cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProvinceResponse>(), It.IsAny<TimeSpan?>()),
            Times.Never);
    }

    [Fact]
    public async Task GetProvinceByCodeAsync_WhenFound_SetsCacheAndReturnsSuccess()
    {
        var m = new LocationUnitOfWorkMock();
        var entity = new Province
        {
            Code = "01",
            Name = "Hà Nội",
            AdministrativeRegionId = 3,
            AdministrativeUnitId = 1,
            AdministrativeRegion = new AdministrativeRegion { Id = 3, Name = "R" },
            AdministrativeUnit = new AdministrativeUnit { Id = 1, Name = "U", Abbreviation = "TP" }
        };
        m.Provinces.Setup(p => p.GetByCodeAsync("01")).ReturnsAsync(entity);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        var key = CacheKeys.ProvinceByCode("01");
        cache.Setup(c => c.GetAsync<ProvinceResponse>(key)).ReturnsAsync((ProvinceResponse?)null);
        cache.Setup(c => c.SetAsync(key, It.IsAny<ProvinceResponse>(), CacheKeys.DefaultCacheDuration))
            .Returns(Task.CompletedTask);

        var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetProvinceByCodeAsync("01");

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy thông tin tỉnh thành công");
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("01");
        result.Data.Name.Should().Be("Hà Nội");
        result.Data.AdministrativeRegionId.Should().Be(3);
        result.Data.AdministrativeRegionName.Should().Be("R");
        result.Data.AdministrativeUnitId.Should().Be(1);
        result.Data.AdministrativeUnitName.Should().Be("U");
        cache.Verify(c => c.SetAsync(key, It.IsAny<ProvinceResponse>(), CacheKeys.DefaultCacheDuration), Times.Once);
    }

    [Fact]
    public async Task GetWardsByProvinceAsync_WhenProvinceMissing_ReturnsNotFound()
    {
        var m = new LocationUnitOfWorkMock();
        m.Provinces.Setup(p => p.GetByCodeAsync("99")).ReturnsAsync((Province?)null);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);

        var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetWardsByProvinceAsync("99");

        LocationServiceResponseAssert.AssertFailureEnvelope(result, 404, "Tỉnh không tồn tại");
        m.Wards.Verify(w => w.GetByProvinceCodeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetWardsByProvinceAsync_WhenProvinceExists_LoadsWardsAndCachesByProvinceCode()
    {
        var m = new LocationUnitOfWorkMock();
        var province = new Province { Code = "01", Name = "Hà Nội", AdministrativeRegionId = 3, AdministrativeUnitId = 1 };
        m.Provinces.Setup(p => p.GetByCodeAsync("01")).ReturnsAsync(province);
        var wardEntities = new List<Ward>
        {
            new()
            {
                Code = "00004",
                Name = "Ba Đình",
                ProvinceCode = "01",
                AdministrativeUnitId = 3,
                Province = province,
                AdministrativeUnit = new AdministrativeUnit { Id = 3, Name = "Phường", Abbreviation = "Phường" }
            }
        };
        m.Wards.Setup(w => w.GetByProvinceCodeAsync("01")).ReturnsAsync(wardEntities);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        var wardsKey = CacheKeys.WardsOfProvince("01");
        cache.Setup(c => c.GetAsync<List<WardResponse>>(wardsKey)).ReturnsAsync((List<WardResponse>?)null);
        cache.Setup(c => c.SetAsync(wardsKey, It.IsAny<List<WardResponse>>(), CacheKeys.DefaultCacheDuration))
            .Returns(Task.CompletedTask);

        var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetWardsByProvinceAsync("01");

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách phường/xã thành công");
        result.Data.Should().NotBeNull().And.HaveCount(1);
        var w = result.Data![0];
        w.Code.Should().Be("00004");
        w.Name.Should().Be("Ba Đình");
        w.ProvinceCode.Should().Be("01");
        w.ProvinceName.Should().Be("Hà Nội");
        w.AdministrativeUnitId.Should().Be(3);
        w.AdministrativeUnitName.Should().Be("Phường");
        m.Wards.Verify(x => x.GetByProvinceCodeAsync("01"), Times.Once);
    }

    [Fact]
    public async Task GetWardsByProvinceAsync_WhenWardsCached_StillResolvesProvinceThenReturnsCache()
    {
        var m = new LocationUnitOfWorkMock();
        var province = new Province { Code = "01", Name = "Hà Nội", AdministrativeRegionId = 3, AdministrativeUnitId = 1 };
        m.Provinces.Setup(p => p.GetByCodeAsync("01")).ReturnsAsync(province);

        var cachedWards = new List<WardResponse> { new() { Code = "00004", Name = "Ba Đình", ProvinceCode = "01" } };
        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.GetAsync<List<WardResponse>>(CacheKeys.WardsOfProvince("01"))).ReturnsAsync(cachedWards);

        var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

        var result = await sut.GetWardsByProvinceAsync("01");

        LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách phường/xã thành công");
        result.Data.Should().BeSameAs(cachedWards);
        m.Provinces.Verify(p => p.GetByCodeAsync("01"), Times.Once);
        m.Wards.Verify(w => w.GetByProvinceCodeAsync(It.IsAny<string>()), Times.Never);
    }
}
