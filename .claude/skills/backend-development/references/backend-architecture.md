# Backend Architecture (2025)

## The Core Question: What Belongs Where?

Architecture is about managing change. Every structural decision — layers, service splits, event-driven — is ultimately an answer to "how do we change this later without breaking everything else?"

---

## Clean Architecture — The Why

The layered structure (Domain → Application → Infrastructure → Host) exists to protect business logic from infrastructure details. The business logic shouldn't care if you use PostgreSQL or MongoDB, REST or gRPC, EF Core or Dapper.

```
Domain        — what the business IS (entities, rules, interfaces)
Application   — what the business DOES (use cases, orchestration)
Infrastructure — how data is stored/fetched (EF Core, HTTP clients, queues)
Host          — how requests come in (Minimal API, background jobs)
```

**Dependency direction**: Domain knows nothing about Infrastructure. Infrastructure knows about Domain. This lets you swap implementations without touching business rules.

**When to simplify**: Not every service needs 4 projects. A service with 3 endpoints and no complex domain logic (like Identity) is better served by a single project than over-engineering it into layers. Apply CA where domain complexity justifies it.

---

## Microservice Boundaries — Drawing the Lines

A service boundary should match a domain boundary, not a technical one. The question isn't "should I split this into microservices?" — it's "what changes independently?"

**Signs a boundary is right**:
- The service can be deployed, scaled, and failed independently
- Teams can own it without coordinating with others constantly
- Data it owns isn't needed directly by other services (only via API/events)

**Signs a boundary is wrong**:
- Services are chatty — every operation requires 3+ inter-service calls
- A "transaction" must span multiple services to be consistent
- One team owns all the services involved

**Current project**: Identity / Vehicle / Media / Notification / AI each own their domain data. They share no database. When Vehicle needs a user's email, it asks Identity via HTTP — it doesn't reach into Identity's DB.

---

## Sync vs Async Communication

The choice between HTTP and message queues is about coupling and failure tolerance, not just performance.

| Use HTTP (sync) when | Use events (async) when |
|----------------------|-------------------------|
| The caller needs the result to proceed | The side effect doesn't affect the response |
| Consistency matters more than availability | Eventual consistency is acceptable |
| Simple lookup from another service | Fan-out to multiple consumers |
| Real-time user-facing read | Background processing, notifications |

**Failure modes differ**: HTTP fails fast (timeout, error). Events are durable but delayed. Design accordingly — don't mix them carelessly.

---

## Repository + Unit of Work

The repository abstracts data access so Application services don't know about EF Core. Unit of Work ensures related operations commit together.

The real benefit isn't testability (though that's nice) — it's **keeping query complexity out of business logic**. When a service method contains a 15-line LINQ query, that's a signal the query belongs in a repository method with a meaningful name.

**When to question it**: Very simple services sometimes find the repo abstraction adds ceremony without value. A direct DbContext injection in a small Application service is acceptable if the team agrees. Consistency within a service matters more than dogmatic adherence.

---

## Bootstrapping Pattern

Keep `Program.cs` a navigation map, not an implementation dump. The pattern:

```csharp
// Program.cs — shows intent, not detail
builder.Services.AddVehicleServices(builder.Configuration);
builder.Services.AddVehicleInfrastructure(builder.Configuration);
app.MapVehicleEndpoints();
```

Each `Add*` or `Map*` method in `Bootstrapping/` is a cohesive group of registrations. This makes it easy to find where a service is registered, and easy to move groups of registrations if the structure changes.

---

## When to Evolve the Structure

Architecture should grow with the system, not be over-designed upfront:

- **Start simple** — a single project is fine until it becomes painful to navigate
- **Extract when friction appears** — if infrastructure changes keep breaking business logic, add a layer
- **Split services when teams diverge** — service splits follow team ownership, not just technical lines
- **Add events when you find yourself writing notification logic inside core business logic**

The current structure reflects where the project is now. Question it when it stops serving the team.
