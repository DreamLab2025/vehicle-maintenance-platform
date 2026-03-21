# Performance Reference — .NET / EF Core

To apply query hygiene, N+1 prevention, caching, and async patterns. Applies to any EF Core backend.

---

## Query Hygiene

### Always paginate lists

```csharp
// Use GetPagedAsync — never GetAllAsync on production data
var (items, total) = await _unitOfWork.Projects.GetPagedAsync(
    pageNumber, pageSize,
    filter: p => p.DeletedAt == null,
    orderBy: q => q.OrderByDescending(p => p.CreatedAt),
    tracking: false);   // read-only → no tracking
```

### No-tracking for reads

```csharp
// tracking: false → EF skips change tracking → faster, less memory
var entity = await _unitOfWork.Projects.GetByIdAsync(id, tracking: false);

// tracking: true only when you need SaveChanges to auto-detect changes
var entity = await _unitOfWork.Projects.GetByIdAsync(id, tracking: true);
entity.Status = "active";
await _unitOfWork.SaveChangesAsync(ct);
```

### Always filter soft-deleted rows

```csharp
filter: p => p.DeletedAt == null   // every query, without exception
```

---

## N+1 Prevention

**The problem:** 1 query to load a list, then 1 query per item for a related entity.

**Fix 1 — Eager load with Include:**

```csharp
var (items, total) = await _unitOfWork.Projects.GetPagedAsync(
    pageNumber, pageSize,
    includes: q => q.Include(p => p.Semester).Include(p => p.Members));
```

**Fix 2 — Projection (select only what you need):**

```csharp
// Avoids loading full entity when only a few fields are needed
var summaries = await _context.Projects
    .Where(p => p.DeletedAt == null)
    .Select(p => new ProjectSummaryResponse(p.Id, p.Name, p.Status))
    .ToListAsync(ct);
```

**Fix 3 — AsQueryable() for complex joins:**

```csharp
var query = _context.Projects
    .Where(p => p.DeletedAt == null && p.SemesterId == semesterId)
    .Include(p => p.Members)
    .OrderByDescending(p => p.CreatedAt);

var total = await query.CountAsync(ct);
var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);
```

---

## Caching with ICacheService (Redis)

Use cache-aside pattern. Cache stable reference data, not user-specific or security-sensitive data.

```csharp
// Read: check cache first, fallback to DB
const string key = "semesters:all";
var cached = await _cache.GetAsync<List<SemesterResponse>>(key);
if (cached is not null) return ApiResponse<...>.SuccessResponse(cached);

var data = /* DB query */;
await _cache.SetAsync(key, data, TimeSpan.FromMinutes(30));
return ApiResponse<...>.SuccessResponse(data);

// Write: always invalidate after mutation
await _unitOfWork.SaveChangesAsync(ct);
await _cache.RemoveAsync("semesters:all");
```

**Cache-worthy:** stable reference data (e.g. semesters, departments), role/permission mapping. In this repo: role_permissions.
**Do NOT cache:** OTPs, user-owned resources, anything where stale = incorrect behavior.

---

## Async Patterns

```csharp
// Never block the thread
var x = task.Result;    // WRONG — deadlock risk
var x = await task;     // correct

// Always propagate CancellationToken
Task<T> GetAsync(Guid id, CancellationToken ct = default)
await _repo.GetAsync(id, ct);   // pass through

// Parallel independent calls
await Task.WhenAll(taskA, taskB);
var a = await taskA; var b = await taskB;

// Sequential dependent calls
var project = await _unitOfWork.Projects.GetByIdAsync(id, ct);
if (project is null) return ...;
var semester = await _unitOfWork.Semesters.GetByIdAsync(project.SemesterId, ct);
```

---

## EF Core Indexes (Configurations)

Always index columns used in `WHERE`, `ORDER BY`, or `JOIN`. Use partial indexes for soft-delete tables.

```csharp
// Partial index — only indexes active rows, much smaller
builder.HasIndex(p => p.Status)
    .HasFilter("deleted_at IS NULL");

builder.HasIndex(p => p.SemesterId)
    .HasFilter("deleted_at IS NULL");

// Composite index for multi-column filters
builder.HasIndex(p => new { p.SemesterId, p.Status });

// Unique with exclusion of nulls and deleted
builder.HasIndex(p => p.Code)
    .IsUnique()
    .HasFilter("code IS NOT NULL AND deleted_at IS NULL");
```

See `docs/design/05-indexes-and-seeds.md` for the full index list.

---

## SaveChanges Discipline

```csharp
// One SaveChangesAsync per logical operation — not per entity
await _unitOfWork.Projects.AddAsync(project);
await _unitOfWork.Members.AddAsync(member);
await _unitOfWork.SaveChangesAsync(ct);   // single round-trip to DB

// Use transaction only when you have multiple SaveChanges calls
await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
try {
    await _unitOfWork.SaveChangesAsync(ct);  // step 1
    await _unitOfWork.SaveChangesAsync(ct);  // step 2
    await tx.CommitAsync(ct);
} catch {
    await tx.RollbackAsync(ct);
    throw;
}
```

---

## Anti-Pattern Checklist

| Anti-pattern                                    | Fix                             |
| ----------------------------------------------- | ------------------------------- |
| `GetAllAsync()` on large table                  | `GetPagedAsync()`               |
| `tracking: true` for reads                      | `tracking: false`               |
| Missing `DeletedAt == null` filter              | Add to every query              |
| Loop + separate DB call per item                | `Include()` or projection       |
| `.Result` / `.Wait()`                           | `await`                         |
| No index on FK or filter column                 | Add in EF configuration         |
| Cache without invalidation on write             | `RemoveAsync()` after save      |
| Multiple `SaveChangesAsync` without transaction | Wrap in `BeginTransactionAsync` |
