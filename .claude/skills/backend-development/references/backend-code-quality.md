# Backend Code Quality (2025)

## Quality is About Changeability, Not Cleverness

Code quality isn't measured by how impressive it looks — it's measured by how easily a *different* engineer (or you, 6 months later) can understand, change, and extend it safely. Every quality practice below serves that goal.

---

## SOLID as a Compass, Not a Checklist

SOLID principles describe forces that make code resist change badly. Apply them when you feel friction, not mechanically for every class.

**Single Responsibility** — A class should have one reason to change. When you find yourself saying "this service handles X *and* Y *and* Z," that's the smell. The fix: split by what changes independently. In practice: validators in their own files, mapping logic separate from business logic, services per aggregate.

**Open/Closed** — Extend behavior without modifying existing code. In this project: add a new `PartCategory` entry instead of adding a new `if` branch. Add a new notification channel via the factory pattern instead of modifying the existing sender. When you're adding `if (type == "new_type")` in 5 places, that's the signal to reach for this principle.

**Liskov Substitution** — An implementation should be usable wherever its interface is expected. Practically: if your `VehicleRepository` implementation throws exceptions the interface contract doesn't mention, or returns slightly different results for edge cases, you've violated LSP. Make implementations behave exactly as the interface promises.

**Interface Segregation** — Don't force callers to depend on methods they don't use. A single `IVehicleService` with 20 methods is worse than two focused interfaces. Callers only depend on what they need — easier to mock, easier to test, easier to understand.

**Dependency Inversion** — Application code depends on abstractions (interfaces), not implementations. Infrastructure *implements* those abstractions. This is why `IUserVehicleRepository` lives in Domain and `UserVehicleRepository : EfUserVehicleRepository` lives in Infrastructure. You can replace EF Core with Dapper without touching business logic.

---

## Naming — Communication, Not Convention

Good names remove the need for comments. Before settling on a name, ask: *does this name tell the next engineer what the thing does and why it exists?*

```
GetUserVehiclesAsync       — clear: returns multiple, async
ApplyTrackingConfigAsync   — clear: applies config, not creates or updates
SyncMaintenanceReminders   — clear: synchronizes (implies idempotent)
DoStuff                    — unclear: what stuff? for whom? when?
HandleRequest              — unclear: every handler "handles a request"
```

Conventions used in this project (follow for consistency, question for improvements):
- `PascalCase` for types and methods
- `_camelCase` for private fields
- `camelCase` for local variables and parameters
- `I` prefix for interfaces
- `Async` suffix for async methods
- `*Apis.cs` for endpoint files, `*Service.cs` for application services

---

## Nullable Reference Types — Explicit Intent

Enabling nullable reference types is a forcing function for being honest about optionality. When you write `string?`, you're documenting a design decision: "this can legitimately be absent." When you write `string`, you're promising it won't be null.

The discipline: don't suppress nullable warnings with `!` unless you genuinely know the value can't be null (e.g., EF Core setting it during hydration). Each `!` you add is a promise you're making to the compiler — break that promise and you get a NullReferenceException in production.

---

## Soft Deletes — Preserve History, Filter by Default

Hard deletes destroy information. Soft deletes (`IsDeleted = true`) preserve the audit trail while hiding deleted records from normal queries.

The key implementation detail is the **global query filter** on DbContext — it means queries automatically exclude soft-deleted records without every developer having to remember to add `&& !IsDeleted` to every query.

```csharp
modelBuilder.Entity<UserVehicle>().HasQueryFilter(e => !e.IsDeleted);
```

**When to question soft deletes**: When the table grows large and the soft-deleted rows become a performance problem. When GDPR/privacy laws require actual data deletion. When the "deleted" concept doesn't make sense for the domain (e.g., a log entry). Soft delete is a default, not a universal law.

---

## Mapping — Explicit Transformations

The choice of mapping approach is a tradeoff between **boilerplate** and **visibility**:

- **Static extension methods** (`vehicle.ToDto()`) — explicit, compile-time safe, discoverable, verbose
- **AutoMapper** — less code, but configuration lives far from usage, silent mismatches at runtime
- **Manual inline** — works for one-offs, but copy-paste error risk at scale

Current preference: static extension methods. When a mapping is wrong, the compiler tells you immediately and you see exactly what's happening. As the project grows, if mapping becomes a significant time sink, revisit — but make the tradeoff consciously.

---

## DI Registration — Organize by Cohesion

`Program.cs` should read like a table of contents, not an implementation file. Group related registrations into focused extension methods:

```csharp
// Good — intent is clear at a glance
builder.Services.AddVehicleDomain();
builder.Services.AddVehicleApplication();
builder.Services.AddVehicleInfrastructure(config);
app.MapVehicleEndpoints();
```

The grouping follows the layers. Each method is a cohesive unit — adding a new service means touching one focused file, not hunting through Program.cs.

---

## Recognizing Code Smells — When to Refactor

Don't refactor pre-emptively. Refactor when you feel the friction:

| Smell | Signal | Consider |
|-------|--------|----------|
| **Anemic domain** | Business logic in services, entities are just bags of properties | Move logic to entity methods |
| **God service** | Service file >400 lines, handles multiple aggregates | Split by aggregate |
| **Magic strings** | `"engine_oil"`, `"critical"` scattered through code | Extract to constants or enums |
| **Deep nesting** | 4+ levels of `if`/`for` nesting | Extract methods, early returns |
| **Shotgun surgery** | One change requires edits in 8 different files | Find the abstraction that unifies them |
| **Feature envy** | Method uses more of another class than its own | Move the method closer to the data it uses |

These are signals to investigate, not automatic triggers. Sometimes the "smell" exists for good reason — understand before refactoring.
