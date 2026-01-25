---
description: "Comprehensive Guidelines for building .NET backend applications"
applyTo: "**/*.cs"
---

# .NET Backend Development Guidelines

You are an expert Senior .NET Backend Engineer. When generating code, you must strictly follow the architectural patterns, coding standards, and best practices defined below.

## 0. WORKFLOW & PLANNING

- **PLAN BEFORE CODING**: Before writing any code, you **MUST** provide a clear, step-by-step plan.
  - **Analysis**: Analyze the requirements and edge cases.
  - **Structure**: Outline changes strictly following the 4 layers: Domain, Application, Infrastructure, API.
  - **Data Flow**: Describe the flow (Request -> Endpoint -> Service -> Repository -> Database).
  - **Verification**: Wait for user confirmation if the logic is complex.

## 1. Architectural Standards

- **Layering Strategy**: Organize the solution into four distinct concentric layers. Dependencies must only point inwards.
  1.  **Domain (Core)**:
      - Contains Entities, Value Objects, Aggregates, Domain Events, Domain Exceptions, and Repository Interfaces.
      - **NO** external dependencies (no NuGet packages for infra/data).
  2.  **Application (Core)**:
      - Contains Use Cases (Services/Handlers), DTOs, Validators, Mapper Interfaces, and Application Exceptions.
      - Orchestrates domain logic but contains no business rules itself.
  3.  **Infrastructure**:
      - Implements interfaces defined in Core (Repositories, Email Services, Storage).
      - Contains DbContext, EF Core configurations, and third-party SDK implementations.
  4.  **API (Presentation)**:
      - Entry point (Minimal APIs).
      - Registers Dependency Injection (DI).
      - Handles HTTP concerns (Status codes, Headers).
- **Dependency Injection**:
  - Use **Primary Constructors** (C# 12+) for injection.
  - Register dependencies in `Program.cs` (API layer) but keep the registration logic defined in extension methods within the Infrastructure/Application layers (e.g., `services.AddInfrastructure()`).
- **Encapsulation**:
  - Domain entities must use private setters. State changes must happen via public methods to ensure invariants.
  - Classes should be `sealed` by default.

## 2. API Design (Minimal APIs)

- **Endpoint Definitions**: Use `app.MapGroup` in the API layer.
- **Service Delegation**: Endpoints should be thin. Immediately delegate work to an `IService` or `IMediator` handler from the Application layer.
- **Documentation**:
  - Use `.Produces<TResponse>()` and `.ProducesProblem()` metadata.
  - Include `.WithSummary()` and `.WithDescription()`.
- **DTOs**:
  - **Strict Separation**: Never accept or return Domain Entities. Always use DTOs defined in the Application layer.
  - **Immutability**: Use `sealed record` for all DTOs.
- **Validation**: Use `IEndpointFilter` to validate DTOs automatically using FluentValidation before reaching the handler.

## 3. Data Access & EF Core

- **Repository Pattern**:
  - Interfaces (`IRepository`) reside in **Domain**.
  - Implementations reside in **Infrastructure**.
- **Performance Rules**:
  - **Read-Only**: Always use `.AsNoTracking()` for queries (Get/List).
  - **Bulk Updates**: Use `.ExecuteUpdateAsync()` and `.ExecuteDeleteAsync()` for batch operations to avoid memory overhead.
  - **Split Queries**: Use `.AsSplitQuery()` for loading collections to prevent Cartesian explosion.
- **Projections**: Always project to DTOs using `.Select()` inside the repository or query handler when possible.

## 4. C# Coding Style

- **Modern Features**: Use C# 12/13/14 features.
- **Pattern Matching**: Extensive use of `is`, `switch` expressions, and property patterns.
- **Null Safety**:
  - Strict `<Nullable>enable</Nullable>`.
  - Use `ArgumentNullException.ThrowIfNull(param)`.
- **Collections**:
  - Use collection expressions: `List<string> list = ["a", "b"];`.
  - Return `IReadOnlyList<T>` or `IEnumerable<T>` to expose data safely.
- **Async/Await**: strictly async from top to bottom. Always propagate `CancellationToken`.

## 5. Validation & Error Handling

- **FluentValidation**: Place validators in the **Application** layer.
- **Global Handling**:
  - Use `IExceptionHandler` middleware in the API layer.
  - Map Domain Exceptions (e.g., `OrderNotFoundException`) to HTTP Status Codes (e.g., 404 Not Found).
  - Return standardized `ProblemDetails` (RFC 7807).

## 6. Naming Conventions

- **PascalCase**: Classes, Methods, Properties.
- **camelCase**: Private fields (`_repo`), parameters, locals.
- **Interfaces**: `I{Name}{Type}` (e.g., `IOrderService`, `ICustomerRepository`).
- **Implementations**: `{Name}{Type}` (e.g., `OrderService`, `CustomerRepository`).
- **Tests**: `MethodName_Condition_Expectation`.

## 7. Configuration & Secrets

- **Options Pattern**: Use strongly-typed `IOptions<T>` settings.
- **Definition**: Settings classes reside in **Application** or **Infrastructure** depending on usage.
- **Validation**: Validate configuration on startup using `ValidateOnStart()`.

## 8. Resilience (Polly)

- **Infrastructure Layer**: Implement resilience policies (Retry, Circuit Breaker, Timeout) within the Infrastructure layer for external calls (DB, APIs).
- **Resilience Pipelines**: Use the modern `ResiliencePipeline` API (Microsoft.Extensions.Resilience).

## 9. Authentication & Authorization

- **JWT**: Implement `JwtBearer` authentication.
- **Policies**: Use Policy-based authorization definitions in `Program.cs`.
- **Current User**: Implement an `ICurrentUserService` interface (in Application) and implementation (in API/Infrastructure) to access claims abstractly.

## 10. Logging & Observability

- **Structured Logging**: Use Serilog.
- **Context**: Always log structured properties: `Log.Information("Processed Order {OrderId}", orderId);`.
- **Tracing**: Prepare for OpenTelemetry by using `ActivitySource` where significant business logic occurs.

## 11. Testing

- **Unit Tests**:
  - Target: **Domain** and **Application** layers.
  - Tools: xUnit, NSubstitute (for mocking Interfaces), FluentAssertions.
- **Integration Tests**:
  - Target: **API** endpoints and **Infrastructure** implementation.
  - Tools: `WebApplicationFactory`, **Testcontainers** (for real DB instances).
- **Structure**: Separate projects `MyProject.UnitTests` and `MyProject.IntegrationTests`.

## 12. Project Structure (Standard Clean Architecture)

Ensure the file structure strictly reflects the layers:

```text
src/
├── MyProject.Domain/           # [Core] Entities, Interfaces, Domain Exceptions
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   └── Repositories/ (Interfaces only)
│
├── MyProject.Application/      # [Core] Use Cases, DTOs, Validators
│   ├── Interfaces/
│   ├── Services/ (or Features/Commands/Queries)
│   ├── DTOs/
│   └── Mappers/
│
├── MyProject.Infrastructure/   # [External] DB, FileSystem, Email
│   ├── Data/ (DbContext, Migrations)
│   ├── Repositories/ (Implementations)
│   └── Services/ (EmailService, etc.)
│
├── MyProject.Api/              # [Entry Point]
│   ├── Endpoints/
│   ├── Middleware/
│   └── Program.cs
│
tests/
├── MyProject.UnitTests/
└── MyProject.IntegrationTests/
```
