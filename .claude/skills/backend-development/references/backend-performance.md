# Backend Performance (2025)

## Measure Before Optimizing

Premature optimization is the root of much unnecessary complexity. The right order is:
1. Make it correct
2. Make it observable (logs, metrics, traces)
3. Find the actual bottleneck
4. Optimize that specific bottleneck

Optimizing a function that runs 10ms when the DB query takes 2s is wasted effort. Profile first, then optimize with data.

---

## Pagination — The Non-Negotiable Default

Unbounded queries are a time bomb. A list endpoint that returns "all" items works fine at 100 records. At 100,000 it OOMs the server. At 1M it times out the DB.

Paginate every list endpoint from day one. The API contract is easier to add pagination to at the start than to retrofit it later when clients are built around unlimited results.

**Good pagination design**:
- Always return `totalCount` so clients know how many pages exist
- Accept `pageNumber` and `pageSize` (not `offset`/`limit` — less prone to bugs)
- Cap `pageSize` at a reasonable maximum (50-100) — validate in the request validator, not the service
- Use consistent defaults: `pageNumber = 1`, `pageSize = 10`

```csharp
// The pattern — same shape everywhere
public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int PageNumber, int PageSize);
```

---

## Async — Understand What You're Doing

`async/await` in .NET is about freeing threads during I/O, not about parallel execution. A blocking call (`.Result`, `.Wait()`) inside an async method defeats the entire purpose — it holds the thread while waiting, causing thread pool starvation under load.

**The subtle mistake**: mixing async and sync in a single call chain:
```csharp
// WRONG — blocks a thread pool thread while the task runs
var result = GetDataAsync().Result;

// RIGHT — thread is freed while waiting
var result = await GetDataAsync();
```

`ConfigureAwait(false)` matters in library code (avoids capturing the synchronization context) but is generally unnecessary in ASP.NET Core handlers where there's no synchronization context.

---

## Caching — Know What You're Caching and Why

Cache when the cost of re-fetching is higher than the cost of staleness. That cost comparison changes by data type:

| Data type | Cache? | TTL reasoning |
|-----------|--------|---------------|
| Vehicle brands, types, models | Yes | Changes rarely, high read frequency |
| Default maintenance schedules | Yes | Content is stable, per-model |
| User vehicles | No | User-specific, changes frequently |
| Maintenance reminders | No | Freshness matters for correctness |

**Cache invalidation strategy**: on every write to cached data, invalidate the cache. Don't rely on TTL alone for data that changes in response to user actions.

**Cache as a layer, not a solution**: caching hides slow queries. If a query is slow, fix the query (add an index, rewrite the join) and then consider caching if the fixed query is still a bottleneck. Caching a broken query is building on a bad foundation.

---

## Background Jobs — Separate "Now" from "Eventually"

Not everything needs to happen in the request-response cycle. The question is: does the user need this result before they can continue?

**In the request cycle** (because the user is waiting):
- Creating a vehicle, maintenance record, odometer update
- Reading vehicle data, reminder status

**In a background job** (because it's a side effect):
- Recalculating reminder levels for all vehicles after a time-based trigger
- Sending notification emails
- Syncing aggregated stats

**The anti-pattern**: doing bulk operations inside API handlers. An endpoint that touches 1000 records is an endpoint that will time out under load. Move bulk work to jobs.

---

## Query Efficiency — Think About What You're Fetching

The most common performance mistake is N+1 queries: fetching a list of 100 items, then making 100 individual queries to fetch related data.

```csharp
// N+1 — 1 query for vehicles + N queries for models
var vehicles = await _context.UserVehicles.Where(...).ToListAsync();
foreach (var v in vehicles) {
    var model = await _context.VehicleModels.FindAsync(v.VehicleModelId); // N queries
}

// 1 query — include what you need upfront
var vehicles = await _context.UserVehicles
    .Include(v => v.VehicleModel)
    .Where(...)
    .ToListAsync();
```

**But don't over-include**: loading every navigation property "just in case" is wasteful. For list queries, project to a DTO shape using `.Select()` — only fetch the columns you'll actually use.

**Global query filters** (like soft delete) run on every query transparently. Know they exist. If you ever need to query soft-deleted records (admin tools, migrations), use `.IgnoreQueryFilters()`.

---

## Selective Sync — Compute When It Changes, Not When It's Read

The maintenance reminder system demonstrates a useful pattern: compute and store reminder levels when the *input* changes (odometer update, maintenance record created), not when they're *read*.

This shifts cost from read-time (many users reading reminders) to write-time (fewer users updating odometers). Reads are instant because the work was already done.

**When this pattern works**: when reads significantly outnumber writes, and the computed value can be stored alongside the source data.

**When it doesn't**: when the computed value depends on too many inputs that change independently, making invalidation complex. In that case, computing at read time (possibly cached) may be simpler.

---

## Signals That Performance Needs Attention

Don't wait for users to complain. Watch for these signals in logs and metrics:

- API response times consistently above 200ms for simple reads
- DB query counts per request growing over time (N+1 emerging)
- Memory usage growing without bound (connection leaks, uncancelled streams)
- Background job queue depth increasing over time (jobs slower than new arrivals)
- Cache hit rate below 80% for data you expected to be cached heavily

Each signal points to a specific investigation, not a generic "make it faster" effort.
