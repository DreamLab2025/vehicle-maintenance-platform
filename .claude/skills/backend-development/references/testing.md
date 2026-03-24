# Backend Testing — Verendar / C# / .NET

To apply test patterns for Verendar services. Reference implementation: `Location/Verendar.Location.Tests`.

**Stack:** xUnit + Moq + FluentAssertions + `Microsoft.Extensions.Logging.Abstractions`.

---

## What to Test

Primary focus is **unit tests for services** — mock `IUnitOfWork` + `ICacheService`, test business logic in isolation.

```
Unit tests (services, validators)  → Verendar.{Service}.Tests/
  ├── Services/                    → one file per service
  └── Support/                     → UnitOfWorkMock, ServiceResponseAssert helpers
```

Run tests:

```bash
task test PROJECT=Location/Verendar.Location.Tests
task test:all
task test:coverage PROJECT=Location/Verendar.Location.Tests
```

---

## Helper Pattern: {Service}UnitOfWorkMock

Create one mock helper per service that wires `IUnitOfWork` + all `IXxxRepository` mocks:

```csharp
// Verendar.Location.Tests/Support/LocationUnitOfWorkMock.cs
internal sealed class LocationUnitOfWorkMock
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Loose);
    public Mock<IProvinceRepository> Provinces { get; } = new(MockBehavior.Strict);
    public Mock<IWardRepository> Wards { get; } = new(MockBehavior.Strict);

    public LocationUnitOfWorkMock()
    {
        UnitOfWork.Setup(u => u.Provinces).Returns(Provinces.Object);
        UnitOfWork.Setup(u => u.Wards).Returns(Wards.Object);
    }
}
```

**Key conventions:**

- `IUnitOfWork` mock: `MockBehavior.Loose` (saves `SaveChangesAsync` etc. don't need explicit setup)
- Repository mocks: `MockBehavior.Strict` — every call must be declared; missing setups throw
- `ICacheService`: `MockBehavior.Strict` — declare every `GetAsync` and `SetAsync` call explicitly

---

## Helper Pattern: {Service}ServiceResponseAssert

Static helper for asserting `ApiResponse<T>` envelope fields:

```csharp
// Verendar.Location.Tests/Support/LocationServiceResponseAssert.cs
internal static class LocationServiceResponseAssert
{
    public static void AssertSuccessEnvelope<T>(ApiResponse<T> response, string expectedMessage)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be(expectedMessage);
        response.Metadata.Should().BeNull();
    }

    public static void AssertCreatedEnvelope<T>(ApiResponse<T> response, string expectedMessage)
    {
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(201);
        response.Message.Should().Be(expectedMessage);
    }

    public static void AssertFailureEnvelope<T>(ApiResponse<T> response, int expectedStatusCode, string expectedMessage)
    {
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(expectedStatusCode);
        response.Message.Should().Be(expectedMessage);
        response.Data.Should().BeNull();
        response.Metadata.Should().BeNull();
    }
}
```

---

## Test Structure (AAA)

Naming: `MethodName_WhenCondition_ExpectedBehavior`

### Cache hit — skip repository

```csharp
[Fact]
public async Task GetAllProvincesAsync_WhenCacheHit_ReturnsCachedAndSkipsRepository()
{
    // Arrange
    var m = new LocationUnitOfWorkMock();
    var cached = new List<ProvinceResponse> { new() { Code = "01", Name = "Hà Nội" } };
    var cache = new Mock<ICacheService>(MockBehavior.Strict);
    cache.Setup(c => c.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll))
         .ReturnsAsync(cached);

    var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

    // Act
    var result = await sut.GetAllProvincesAsync();

    // Assert
    LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách tỉnh thành công");
    result.Data.Should().BeSameAs(cached);
    m.Provinces.Verify(p => p.GetAllAsync(), Times.Never);  // repo not called
}
```

### Cache miss — load from DB, set cache

```csharp
[Fact]
public async Task GetAllProvincesAsync_WhenCacheMiss_LoadsFromRepositoryAndSetsCache()
{
    // Arrange
    var m = new LocationUnitOfWorkMock();
    var entities = new List<Province> { new() { Code = "01", Name = "Hà Nội" } };
    m.Provinces.Setup(p => p.GetAllAsync()).ReturnsAsync(entities);

    var cache = new Mock<ICacheService>(MockBehavior.Strict);
    cache.Setup(c => c.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll))
         .ReturnsAsync((List<ProvinceResponse>?)null);
    cache.Setup(c => c.SetAsync(CacheKeys.ProvincesAll, It.IsAny<List<ProvinceResponse>>(), CacheKeys.DefaultCacheDuration))
         .Returns(Task.CompletedTask);

    var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

    // Act
    var result = await sut.GetAllProvincesAsync();

    // Assert
    LocationServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách tỉnh thành công");
    result.Data.Should().HaveCount(1);
    m.Provinces.Verify(p => p.GetAllAsync(), Times.Once);
    cache.Verify(c => c.SetAsync(CacheKeys.ProvincesAll, It.IsAny<List<ProvinceResponse>>(), CacheKeys.DefaultCacheDuration), Times.Once);
}
```

### Not found — 404

```csharp
[Fact]
public async Task GetProvinceByCodeAsync_WhenNotFound_Returns404()
{
    var m = new LocationUnitOfWorkMock();
    var cache = new Mock<ICacheService>(MockBehavior.Strict);
    cache.Setup(c => c.GetAsync<ProvinceResponse>(CacheKeys.ProvinceByCode("99")))
         .ReturnsAsync((ProvinceResponse?)null);
    m.Provinces.Setup(p => p.GetByCodeAsync("99")).ReturnsAsync((Province?)null);

    var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

    var result = await sut.GetProvinceByCodeAsync("99");

    LocationServiceResponseAssert.AssertFailureEnvelope(result, 404, "Không tìm thấy tỉnh");
}
```

### Exception → failure envelope

```csharp
[Fact]
public async Task GetAllProvincesAsync_WhenRepositoryThrows_ReturnsFailure()
{
    var m = new LocationUnitOfWorkMock();
    m.Provinces.Setup(p => p.GetAllAsync()).ThrowsAsync(new InvalidOperationException("db error"));

    var cache = new Mock<ICacheService>(MockBehavior.Strict);
    cache.Setup(c => c.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll))
         .ReturnsAsync((List<ProvinceResponse>?)null);

    var sut = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);

    var result = await sut.GetAllProvincesAsync();

    LocationServiceResponseAssert.AssertFailureEnvelope(result, 400, "Lỗi khi lấy danh sách tỉnh");
}
```

### Write operation — verify AddAsync + SaveChangesAsync

```csharp
[Fact]
public async Task CreateGarageAsync_WhenNewOwner_CreatesGarageAndReturns201()
{
    // Arrange
    var ownerId = Guid.NewGuid();
    var m = new GarageUnitOfWorkMock();
    m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Garage, bool>>>()))
             .ReturnsAsync((Garage?)null);
    m.Garages.Setup(r => r.AddAsync(It.IsAny<Garage>()))
             .ReturnsAsync((Garage g) => g);

    var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object);

    // Act
    var result = await sut.CreateGarageAsync(ownerId, new GarageRequest { BusinessName = "Garage Alpha" });

    // Assert
    GarageServiceResponseAssert.AssertCreatedEnvelope(result, "Đăng ký garage thành công");
    result.Data!.OwnerId.Should().Be(ownerId);
    m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    m.Garages.Verify(r => r.AddAsync(It.IsAny<Garage>()), Times.Once);
}

[Fact]
public async Task CreateGarageAsync_WhenOwnerAlreadyHasGarage_Returns409()
{
    var m = new GarageUnitOfWorkMock();
    var existing = new Garage { OwnerId = Guid.NewGuid() };
    m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Garage, bool>>>()))
             .ReturnsAsync(existing);

    var sut = new GarageService(NullLogger<GarageService>.Instance, m.UnitOfWork.Object);
    var result = await sut.CreateGarageAsync(existing.OwnerId, new GarageRequest { BusinessName = "Dup" });

    GarageServiceResponseAssert.AssertFailureEnvelope(result, 409, "Tài khoản đã có garage đăng ký");
    m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

---

## Coverage Targets

- **Services:** success path, not-found, conflict/duplicate, exception handling
- **Cache-aware services:** cache hit (repo skipped), cache miss (repo called + cache set)
- **Write operations:** verify `AddAsync`/`SaveChangesAsync` called; verify not called on early return

---

## Best Practices

1. **AAA:** Arrange (mocks + input), Act (call sut), Assert (FluentAssertions).
2. **Names:** `MethodName_WhenCondition_ExpectedBehavior`
3. **`NullLogger<T>.Instance`** instead of `Mock<ILogger<T>>` for logger dependencies.
4. **One logical assertion per test**; multiple `Should()` on the same object is fine.
5. **No shared mutable state** — xUnit creates new class instance per test.
6. **Strict repo mocks** — every unexpectedly called method throws, which prevents silent test passing.
7. **Vietnamese messages** — assert exact Vietnamese message strings that the service returns.

---

## Checklist

- [ ] `{Service}UnitOfWorkMock` helper in `Tests/Support/`
- [ ] `{Service}ServiceResponseAssert` helper in `Tests/Support/`
- [ ] Cache hit test (verifies repo is never called)
- [ ] Cache miss test (verifies repo called once + cache set once)
- [ ] Not-found test (verifies 404 + correct message)
- [ ] Exception test (verifies failure envelope)
- [ ] Write tests: success (AddAsync + SaveChangesAsync called), duplicate (SaveChangesAsync never called)
