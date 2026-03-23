# Performance Reference

## Caching (Location & Identity — services with Redis)

Cache-aside pattern with `ICacheService` from `Verendar.Common.Caching`.

```csharp
public async Task<ApiResponse<List<ProvinceResponse>>> GetAllProvincesAsync()
{
    var cached = await _cache.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll);
    if (cached is not null)
        return ApiResponse<List<ProvinceResponse>>.SuccessResponse(cached, "...");

    var entities = await _unitOfWork.Provinces.GetAllAsync();
    var response  = entities.Select(p => p.ToResponse()).ToList();
    await _cache.SetAsync(CacheKeys.ProvincesAll, response, CacheKeys.DefaultCacheDuration);
    return ApiResponse<List<ProvinceResponse>>.SuccessResponse(response, "...");
}
```

CacheKeys — use schema version suffix to auto-invalidate on DTO shape changes:

```csharp
public static class CacheKeys
{
    private const string Prefix  = "location";
    private const string Version = "v2";   // bump when DTO fields change
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(24);

    public static string ProvincesAll          => $"{Prefix}:provinces:{Version}";
    public static string ById(string code)     => $"{Prefix}:provinces:{code}:{Version}";
    public static string WardsOf(string code)  => $"{Prefix}:provinces:{code}:wards:{Version}";
}
```

Invalidate on writes: `await _cache.RemoveAsync(CacheKeys.ById(id));`

## Pagination (Vehicle, Identity, Media, Notification)

Call `IGenericRepository<T>.GetPagedAsync` directly from the service — no custom repo wrapper needed.

```csharp
// Service
request.Normalize();
var (items, totalCount) = await _unitOfWork.Types.GetPagedAsync(
    request.PageNumber,
    request.PageSize,
    orderBy: request.IsDescending.HasValue
        ? (request.IsDescending.Value
            ? q => q.OrderByDescending(t => t.CreatedAt)
            : q => q.OrderBy(t => t.CreatedAt))
        : null
);
return ApiResponse<List<TypeSummary>>.SuccessPagedResponse(
    items.Select(t => t.ToSummary()).ToList(),
    totalCount, request.PageNumber, request.PageSize, "...");
```

Call `request.Normalize()` first — sets sensible defaults for page/size if not provided.
Never return unbounded lists for mutable data.

## N+1 Prevention

Include navigations the `ToResponse()` needs — in the repository, not lazily.

```csharp
public async Task<Brand?> GetByIdWithTypesAsync(Guid id)
    => await _db.Brands
        .Include(b => b.VehicleType)
        .AsNoTracking()
        .FirstOrDefaultAsync(b => b.Id == id);
```

`AsNoTracking()` on all read-only queries.
