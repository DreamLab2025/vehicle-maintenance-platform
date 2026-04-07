---
name: api-design
description: REST API design patterns for the Verendar project — resource naming, status codes, pagination, filtering, error responses. Includes mandatory Verendar conventions (ApiResponse<T>, PaginationRequest, internal endpoints, soft delete). Use this skill whenever designing or reviewing API endpoints in Verendar. Activate proactively when the user is adding a new endpoint, designing a route group, or asking about API structure.
origin: ECC
---

# API Design — Verendar / .NET

## Verendar Mandatory Conventions

See [`references/dotnet-patterns.md`](references/dotnet-patterns.md) for full C# code. Key rules:

| Rule | Detail |
|------|--------|
| `ApiResponse<T>` on all public endpoints | Hard constraint — no exceptions |
| Internal endpoints skip `ApiResponse<T>` | `/api/internal/...` + `Role = "Service"` auth |
| All lists use `PaginationRequest` | Inherit it, use `[AsParameters]`, call `GetPagedAsync` |
| Route groups in `{Module}Apis.cs` | Static class, private static handlers |
| Soft delete only | `DeletedAt = DateTime.UtcNow`, never `dbContext.Remove()` |
| Errors → RFC 7807 | `Results.Problem(...)`, never expose stack traces |

---

## URL Structure

```
# Public — plural nouns, kebab-case, no verbs
GET    /api/garages
GET    /api/garages/{id}
POST   /api/garages
PATCH  /api/garages/{id}

# Actions (verb acceptable here only)
POST   /api/bookings/{id}/cancel
POST   /api/auth/login

# Internal — service-to-service only
POST   /api/internal/users/{id}/roles
GET    /api/internal/payments/{id}
```

**Avoid**: verbs in resource URLs (`/getUsers`), singular (`/user`), snake_case in paths.

---

## HTTP Methods & Status Codes

| Method | Success | Use For |
|--------|---------|---------|
| GET | 200 | Retrieve one or many |
| POST | 201 + Location header | Create |
| PATCH | 200 | Partial update |
| DELETE | 204 | Remove (soft delete in Verendar) |

```
400 Bad Request       — Validation failure (FluentValidation → ValidationEndpointFilter)
401 Unauthorized      — Missing/invalid JWT
403 Forbidden         — Wrong role
404 Not Found         — Resource doesn't exist or is soft-deleted
409 Conflict          — Duplicate / state conflict
429 Too Many Requests — Rate limit hit
500 Server Error      — Never expose details (RFC 7807 Problem Details)
```

---

## Response Format

**Public** (always wrapped in `ApiResponse<T>`):
```json
{ "success": true,  "data": { "id": "...", "status": "Pending" } }
{ "success": false, "error": { "code": "not_found", "message": "Booking not found" } }
```

**Internal** (plain object, no wrapper):
```json
{ "userId": "...", "role": "GarageOwner" }
```

---

## Pagination

Verendar uses offset-based pagination via `PaginationRequest` + `GetPagedAsync`. Not cursor-based.

```
GET /api/bookings?page=1&pageSize=20&status=Pending&sortBy=scheduledAt
```

All filter + sort params go into one class inheriting `PaginationRequest` — never separate `[FromQuery]` params.

---

## Filtering & Sorting

```
GET /api/garages?search=auto&province=HN&sort=-createdAt
GET /api/bookings?status=Pending,InProgress&garageId={id}
```

- Comma-separated for multiple values: `?status=Pending,InProgress`
- Prefix `-` for descending sort: `?sort=-scheduledAt`

---

## Endpoint Checklist

- [ ] URL: plural noun, kebab-case, no verbs
- [ ] Returns `ApiResponse<T>` (public) or plain object (internal)
- [ ] POST returns 201 + `Location` header; DELETE returns 204
- [ ] List endpoint: request DTO inherits `PaginationRequest` + `[AsParameters]`
- [ ] FluentValidation validator exists for the request DTO
- [ ] `ValidationEndpointFilter` attached on POST/PUT/PATCH
- [ ] `RequireAuthorization()` set (or explicitly marked public)
- [ ] Soft delete used — never `dbContext.Remove()`
- [ ] Errors use `Results.Problem(...)` — no stack traces or exception messages
