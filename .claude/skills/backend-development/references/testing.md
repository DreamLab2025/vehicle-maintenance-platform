# Testing Reference

## Do / Don't

### Mock setup
```csharp
// DO — MockBehavior.Strict on repos; declare only what the test calls
var cache = new Mock<ICacheService>(MockBehavior.Strict);
cache.Setup(c => c.GetAsync<ProvinceResponse>(CacheKeys.ProvinceByCode("99")))
     .ReturnsAsync((ProvinceResponse?)null);
m.Provinces.Setup(p => p.GetByCodeAsync("99")).ReturnsAsync((Province?)null);

// DON'T — MockBehavior.Loose on repos (hides unexpected calls) or set up calls you never verify
var repo = new Mock<IProvinceRepository>();  // ❌ Loose — silently ignores unexpected calls
repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Province>());  // ❌ set up but never called in this test
```

### Asserting the response envelope
```csharp
// DO — use the shared ResponseAssert helper
LocationServiceResponseAssert.AssertFailureEnvelope(result, 404, "Tỉnh không tồn tại");

// DON'T — assert raw StatusCode and string separately in every test
result.StatusCode.Should().Be(404);    // ❌ duplicate logic — use the assert helper
result.IsSuccess.Should().BeFalse();
result.Message.Should().Be("Tỉnh không tồn tại");
```

### Cache verification
```csharp
// DO — verify SetAsync is NOT called on a cache-hit path
cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProvinceResponse>(), It.IsAny<TimeSpan?>()), Times.Never);

// DON'T — skip cache interaction verification
// (no assertion on SetAsync) ❌ — the cache-aside contract goes untested
```

### Test naming
```csharp
// DO
GetProvinceByCodeAsync_WhenNotFound_Returns404

// DON'T
GetProvinceTest          // ❌ no condition or expectation
TestGetProvince_Error    // ❌ vague, reversed convention
```

---


Stack: **xUnit + Moq + FluentAssertions**. Reference: `Location/Verendar.Location.Tests`.

## Project Structure

```
Verendar.{Service}.Tests/
├── Services/{Resource}ServiceTests.cs
├── Serialization/{Service}ApiResponseJsonTests.cs
└── Support/
    ├── {Service}UnitOfWorkMock.cs      # wires IUnitOfWork + all repo mocks
    └── {Service}ServiceResponseAssert.cs
```

## UnitOfWorkMock

```csharp
internal sealed class LocationUnitOfWorkMock
{
    public Mock<IUnitOfWork>          UnitOfWork { get; } = new(MockBehavior.Loose);
    public Mock<IProvinceRepository>  Provinces  { get; } = new(MockBehavior.Strict);

    public LocationUnitOfWorkMock()
    {
        UnitOfWork.Setup(u => u.Provinces).Returns(Provinces.Object);
    }
}
```

`MockBehavior.Strict` on repos — fails if you call a method you didn't set up.
Declare only the repo mocks your test actually touches; don't wire up repos that aren't called.

## ResponseAssert

```csharp
internal static class LocationServiceResponseAssert
{
    public static void AssertSuccessEnvelope<T>(ApiResponse<T> r, string msg)
    {
        r.IsSuccess.Should().BeTrue();
        r.StatusCode.Should().Be(200);
        r.Message.Should().Be(msg);
    }

    public static void AssertFailureEnvelope<T>(ApiResponse<T> r, int code, string msg)
    {
        r.IsSuccess.Should().BeFalse();
        r.StatusCode.Should().Be(code);
        r.Message.Should().Be(msg);
        r.Data.Should().BeNull();
    }
}
```

## Test Naming

`MethodName_WhenCondition_ExpectedBehavior`

## What to Cover

**Read with cache:** cache hit / cache miss+found / not found / exception

**Read without cache:** found / not found / exception

**Write:** happy path (SaveChanges called once) / not found before write / business rule violation / exception

## Example

```csharp
[Fact]
public async Task GetProvinceByCodeAsync_WhenNotFound_Returns404()
{
    var m     = new LocationUnitOfWorkMock();
    var cache = new Mock<ICacheService>(MockBehavior.Strict);
    cache.Setup(c => c.GetAsync<ProvinceResponse>(CacheKeys.ProvinceByCode("99")))
         .ReturnsAsync((ProvinceResponse?)null);
    m.Provinces.Setup(p => p.GetByCodeAsync("99")).ReturnsAsync((Province?)null);

    var sut    = new ProvinceService(NullLogger<ProvinceService>.Instance, m.UnitOfWork.Object, cache.Object);
    var result = await sut.GetProvinceByCodeAsync("99");

    LocationServiceResponseAssert.AssertFailureEnvelope(result, 404, "Tỉnh không tồn tại");
    cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProvinceResponse>(), It.IsAny<TimeSpan?>()), Times.Never);
}
```
