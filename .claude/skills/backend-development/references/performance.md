# Performance Reference

## Do / Don't

### Caching
```csharp
// DO — cache-aside: check cache first, load + store on miss
var cached = await _cache.GetAsync<List<ProvinceResponse>>(CacheKeys.ProvincesAll);
if (cached is not null) return ApiResponse<...>.SuccessResponse(cached, "...");
var entities = await _unitOfWork.Provinces.GetAllAsync();
var response = entities.Select(p => p.ToResponse()).ToList();
await _cache.SetAsync(CacheKeys.ProvincesAll, response, CacheKeys.DefaultCacheDuration);

// DON'T — cache the entity, store without TTL, or skip cache invalidation on writes
await _cache.SetAsync(key, entities);   // ❌ cache the raw entity, not the DTO
await _cache.SetAsync(key, response);   // ❌ no TTL means items never expire
// forgetting:  await _cache.RemoveAsync(CacheKeys.ById(id))  after an update ❌
```

### Pagination
```csharp
// DO — call GetPagedAsync, normalize first
request.Normalize();
var (items, total) = await _unitOfWork.Brands.GetPagedAsync(request.PageNumber, request.PageSize, ...);
return ApiResponse<...>.SuccessPagedResponse(items.Select(b => b.ToSummary()).ToList(), total, ...);

// DON'T — load all then slice in memory, or return unbounded lists for mutable data
var all = await _unitOfWork.Brands.GetAllAsync();          // ❌ unbounded
var page = all.Skip((n-1)*size).Take(size).ToList();       // ❌ slice in memory
return ApiResponse<...>.SuccessResponse(all.Select(b => b.ToSummary()).ToList(), "..."); // ❌ unbounded response
```

### N+1 prevention
```csharp
// DO — Include in the repo method, AsNoTracking on reads
public async Task<Brand?> GetByIdWithTypesAsync(Guid id)
    => await _db.Brands.Include(b => b.VehicleType).AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

// DON'T — navigate lazily or Include in the service/handler
var brand = await _unitOfWork.Brands.GetByIdAsync(id);
var typeName = brand.VehicleType.Name;   // ❌ lazy nav property → N+1 or NullRef
```

---


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
