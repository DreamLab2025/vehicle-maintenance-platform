# Backend API Design (2025)

## APIs Are Contracts, Not Just Code

An API endpoint is a promise to callers. Once published, changing it breaks things. Design with that permanence in mind — before writing a single line, ask:

- What does the caller actually need (not just what's convenient to return)?
- What's the simplest shape that satisfies that need today and allows growth tomorrow?
- What can go wrong, and how should callers know?

---

## Choosing the Right Abstraction Level

**Too granular**: Callers need 4 requests to load one screen — chatty, slow, fragile.
**Too coarse**: One "god endpoint" returns everything, callers filter client-side — wasteful, leaky.

Find the caller's mental model. A mobile app showing a vehicle dashboard needs a response shaped like that dashboard, not a raw dump of database fields.

**Current approach**: Minimal API with `MapGroup` — routes declared alongside handlers in feature-focused files (`UserVehicleApis.cs`). This keeps related endpoints together without the ceremony of controllers, and makes the route tree visible at a glance.

```csharp
// Group by resource, name by action
group.MapGet("/", GetList).RequireAuthorization();
group.MapGet("/{id:guid}", GetById).RequireAuthorization();
group.MapPost("/", Create).RequireAuthorization();
group.MapPatch("/{id:guid}/complete-onboarding", CompleteOnboarding).RequireAuthorization();
```

---

## Route Design Thinking

Routes should express _what a resource is_, not _what code runs_. Think nouns, not verbs.

```
/api/v1/{resource}                     collection
/api/v1/{resource}/{id}                single item
/api/v1/{resource}/{id}/{sub-resource} owned sub-resource
/api/v1/{resource}/{id}/{state-action} state transition (noun form of verb)
```

**State transitions as sub-routes** — prefer `PATCH /vehicles/{id}/complete-onboarding` over `POST /vehicles/complete-onboarding/{id}`. The resource is always the subject.

**Version in the path** — `/api/v1/` makes future breaking changes possible without disrupting existing clients. Start with v1; don't add v2 until you have an actual breaking change.

---

## Validation — Where and How

Validation belongs at the entry point, not scattered through the call stack.

**Structural validation** (FluentValidation at the boundary):

- Required fields present, correct format, within range
- Field correlations (`customPartName` required when no `partProductId`)
- Validators live in `Application/Validators/`, use Vietnamese messages for user-facing errors

**Business validation** (inside the service/domain):

- Does this vehicle belong to this user?
- Is the user allowed to create another vehicle?
- Would this operation violate a domain invariant?

Don't conflate them. FluentValidation shouldn't know about DB state. Services shouldn't re-check field formats.

---

## HTTP Semantics — Use Them Correctly

Correct HTTP semantics make APIs predictable and cache-friendly:

| Method | Intent                             | Idempotent | Safe |
| ------ | ---------------------------------- | ---------- | ---- |
| GET    | Read, no side effects              | Yes        | Yes  |
| POST   | Create or non-idempotent action    | No         | No   |
| PUT    | Full replace                       | Yes        | No   |
| PATCH  | Partial update or state transition | Varies     | No   |
| DELETE | Remove                             | Yes        | No   |

**GET should never mutate state** — caches, proxies, and clients assume GET is safe.

**PATCH for state transitions** — `complete-onboarding`, `odometer`, `activate` are partial updates on resource state. Don't invent `POST /do-something` for these.

---

## Response Shape Consistency

Inconsistent responses are the #1 thing that makes APIs hard to consume. Pick conventions and apply them everywhere.

**Wrapper**: `ApiResponse<T>` (từ `Verendar.Common.Shared`) bọc mọi response:

```json
// Success
{ "isSuccess": true, "message": "Success", "data": { ... }, "metadata": null }

// Paginated — data là List<T>, metadata là PagingMetadata
{ "isSuccess": true, "message": "Success", "data": [ ... ],
  "metadata": { "pageNumber": 1, "pageSize": 10, "totalItems": 3, "totalPages": 1,
                "hasNextPage": false, "hasPreviousPage": false } }

// Business logic failure (not found, forbidden, etc.)
{ "isSuccess": false, "message": "Vehicle not found.", "data": null, "metadata": null }
```

**Exception**: Validation errors (FluentValidation) và unhandled 500 dùng RFC 7807 Problem Details — không qua ApiResponse:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "fieldName": ["message"] }
}
```

**HTTP status codes**:

```
201 Created (write), 200 OK (read/update), 204 No Content (delete/void)
400 (validation), 401 (no auth), 403 (no permission), 404 (not found via FailureResponse)
500 — never expose stack trace or internal detail, include traceId
```

---

## Mapping — Explicit Over Magic

Static extension methods over AutoMapper:

- Compile-time errors when DTO shapes change (not runtime surprises)
- Immediately readable — you see exactly what maps to what
- No configuration drift between mapper setup and usage

```csharp
// Application/Mappings/UserVehicleMappings.cs
public static UserVehicleResponse ToResponse(this UserVehicle v) => new(
    v.Id, v.LicensePlate, v.VinNumber, v.PurchaseDate, v.CurrentOdometer,
    v.NeedsOnboarding, v.VehicleVariant.ToResponse()
);
```

The tradeoff: more code to write. Worth it at this project's scale.

---

## Authorization — Layers of Trust

1. **Route-level**: `.RequireAuthorization()` — authenticated users only
2. **Policy-level**: role checks (`RequireAuthorization("Admin")` for admin-only endpoints)
3. **Resource-level**: does this _specific user_ own this _specific resource_?

The third layer is most commonly missed. Authenticated ≠ authorized for this record.

```csharp
var vehicle = await _repo.GetByIdAsync(vehicleId);
if (vehicle.UserId != currentUserId) return Results.Forbid();
```

---

## Evolving the API

Good APIs are designed to evolve without breaking callers:

- **Add** optional response fields freely — callers ignore unknown fields
- **Add** optional request parameters freely — callers don't have to send them
- **Rename/remove** response fields only with versioning or deprecation notices
- **Change semantics** of existing parameters — almost always a breaking change

When you need a breaking change, introduce `/api/v2/` for that resource, not silently change v1.
