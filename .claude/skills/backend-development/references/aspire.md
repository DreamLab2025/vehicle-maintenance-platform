# Aspire Reference — Verendar Orchestration

To understand how .NET Aspire wires together Verendar services, infrastructure, and configuration. Read this when adding a new service, new infrastructure dependency, inter-service HTTP calls, or diagnosing startup/discovery issues.

---

## Project Layout

```
App/
├── Verendar.AppHost/          ← Orchestration: declares all resources + services
│   └── Extensions/ExternalServiceRegistrationExtensions.cs
├── Verendar.ServiceDefaults/  ← Shared Aspire defaults for every service host
└── Verendar.Common/           ← Shared middleware, DI helpers, JWT, MassTransit
```

---

## AppHost — Registering a Service

Every service (and infrastructure resource) is declared in `AppHost/Extensions/ExternalServiceRegistrationExtensions.cs`.

**Infrastructure resources** (shared across services):

```csharp
var postgres  = builder.AddPostgres("postgres")...;
var rabbitMq  = builder.AddRabbitMQ("rabbitmq")...;
var redis     = builder.AddRedis("redis")...;
var seq       = builder.AddSeq("seq")...;

var garageDb  = postgres.AddDatabase("garage-db");
```

**Service registration:**

```csharp
var garageService = builder.AddProject<Projects.Verendar_Garage>("garage-service")
    .WithReference(garageDb)
    .WithReference(rabbitMq)
    .WithReference(redis)    // only if service uses cache
    .WithReference(seq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(redis);
```

**YARP gateway** — add routes for the new service:

```csharp
var garageCluster = yarp.AddProjectCluster(garageService);
yarp.AddRoute("/api/v1/garages/{**catch-all}", garageCluster);
yarp.AddRoute("/api/v1/bookings/{**catch-all}", garageCluster);
```

Routes for all services are documented in `CLAUDE.md → Gateway Routing`.

---

## ServiceDefaults — Every Host Calls This

`AddServiceDefaults()` is called in every service's `AddApplicationServices()`. It wires:

- OpenTelemetry (tracing, metrics, logging via Serilog + Seq)
- Service discovery (`IServiceDiscovery`)
- HTTP client resilience handlers
- Health check endpoints (`/health`, `/alive`)

```csharp
// Bootstrapping/ApplicationServiceExtensions.cs
public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
{
    builder.AddServiceDefaults();   // ← always first
    builder.AddCommonService();     // CORS, JWT, MassTransit, rate limiting, Swagger
    builder.AddPostgresDatabase<GarageDbContext>(Const.GarageDatabase);
    // ... register services
    return builder;
}

public static WebApplication UseApplicationServices(this WebApplication app)
{
    app.MapDefaultEndpoints();     // /health + /alive
    app.UseCommonService();        // middleware pipeline
    if (app.Environment.IsDevelopment())
        app.UseHangfireDashboard();
    return app;
}
```

---

## Database Connection

Aspire injects the connection string automatically by matching the name in `AddPostgresDatabase<T>` to the database registered in AppHost.

```csharp
// Constants (Verendar.Common or service-local Const.cs)
public const string GarageDatabase = "garage-db";

// In AddApplicationServices:
builder.AddPostgresDatabase<GarageDbContext>(Const.GarageDatabase);
```

This calls `builder.AddNpgsqlDbContext<TContext>(databaseName, ...)` with the `MigrationsAssembly` set to the Infrastructure assembly.

**Migrate on startup** — call this before mapping routes:

```csharp
var app = builder.Build();
await app.MigrateDbContextAsync<GarageDbContext>();
app.UseApplicationServices();
app.MapGarageApi();
await app.RunAsync();
```

---

## Redis Cache

Only register Redis if the service uses `ICacheService` (e.g., Location service).

```csharp
builder.AddServiceRedis("garage-service", connectionName: Const.Redis);
```

This registers `ICacheService` as a singleton, scoped with a service-level key prefix so cache keys are namespaced per service.

---

## Inter-Service HTTP Clients

When one service needs to call another (e.g., Garage calling Payment), use a typed `IHttpClientFactory`-based client:

```csharp
// Application/Services/Interfaces/IPaymentClient.cs
public interface IPaymentClient
{
    Task<PaymentInitResponse?> InitAsync(PaymentInitRequest request, CancellationToken ct);
}

// Infrastructure/HttpClients/PaymentHttpClient.cs
public class PaymentHttpClient(HttpClient client) : IPaymentClient { ... }

// In AddApplicationServices:
builder.Services.AddHttpClient<IPaymentClient, PaymentHttpClient>(client =>
{
    var baseUrl = builder.Configuration["Services:Payment:BaseUrl"]
        ?? "https://localhost:8005";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddHttpMessageHandler<ForwardAuthorizationHandler>(); // propagates JWT token
```

`ForwardAuthorizationHandler` reads the `Authorization` header from the incoming request and forwards it to the downstream service. Register it as a transient service.

---

## MassTransit (Event-Driven)

Services that publish or consume domain events use MassTransit over RabbitMQ. `AddCommonService()` configures MassTransit via auto-scanning.

**Publishing an event:**

```csharp
// In service constructor
public class BookingService(
    ILogger<BookingService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IBookingService { ... }

// In service method
await publishEndpoint.Publish(new BookingConfirmedEvent { BookingId = booking.Id }, ct);
```

**Consuming an event:**

```csharp
// Infrastructure/Consumers/PaymentSucceededConsumer.cs
public class PaymentSucceededConsumer(
    ILogger<PaymentSucceededConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<PaymentSucceededEvent>
{
    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var booking = await unitOfWork.Bookings.GetByIdAsync(context.Message.OrderId);
        if (booking is null) return;
        booking.Status = BookingStatus.Confirmed;
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
```

Consumers are discovered automatically — just place them in any assembly that `AddCommonService()` scans. No manual registration needed.

**Shared contracts** — event types used across service boundaries live in `Verendar.{Service}.Contracts`:

- `Verendar.Identity.Contracts` — auth/user events
- `Verendar.Vehicle.Contracts` — vehicle events

---

## Task Commands

```bash
task run          # start all services + infra via Aspire dashboard
task build        # build entire solution
task clean        # clean build artifacts
task clear        # remove Aspire-managed containers and volumes (full reset)

# Database migrations (per service)
task migrate:add NAME=AddBookingStatus PROJECT=Garage/Verendar.Garage.Infrastructure STARTUP=Garage/Verendar.Garage
task migrate:remove PROJECT=Garage/Verendar.Garage.Infrastructure STARTUP=Garage/Verendar.Garage

# Tests
task test PROJECT=Location/Verendar.Location.Tests
task test:all
task test:coverage PROJECT=Location/Verendar.Location.Tests
```

---

## Development vs. Production

| Concern            | Development                                      | Production                       |
| ------------------ | ------------------------------------------------ | -------------------------------- |
| OpenAPI/Swagger    | Enabled (`app.MapOpenApi()`)                     | Disabled                         |
| Hangfire dashboard | Enabled (no auth guard)                          | Hidden                           |
| Test user seeding  | Identity service seeds test users                | Skipped                          |
| YARP HTTPS verify  | Disabled (`DangerousAcceptAnyServerCertificate`) | Enabled                          |
| CORS               | Allows any origin                                | Restricted to configured origins |
| PgAdmin            | Optional via `VERENDAR_PGADMIN=1` env var        | Not available                    |

---

## Health Checks

Every service exposes two endpoints (from `MapDefaultEndpoints()`):

| Endpoint  | Purpose                             |
| --------- | ----------------------------------- |
| `/health` | Full health check (DB, Redis, etc.) |
| `/alive`  | Liveness probe                      |

Aspire uses these to determine when a service is ready. The `WaitFor()` calls in AppHost depend on these returning healthy before routing traffic.

---

## Checklist: Adding a New Service to Aspire

```
AppHost
[ ] Add database: postgres.AddDatabase("{service}-db")
[ ] Register project: builder.AddProject<Projects.Verendar_{Service}>(...)
    .WithReference(db).WithReference(rabbitMq)...
    .WaitFor(postgres).WaitFor(rabbitMq)...
[ ] Add YARP routes for the new service's API prefix(es)

Service Host
[ ] Call builder.AddServiceDefaults() first in AddApplicationServices()
[ ] Call builder.AddPostgresDatabase<{Service}DbContext>(Const.{Service}Database)
[ ] Optionally call builder.AddServiceRedis() if using ICacheService
[ ] Call await app.MigrateDbContextAsync<{Service}DbContext>() before running
[ ] Call app.MapDefaultEndpoints() in UseApplicationServices()

Constants
[ ] Add database name constant to Const.cs (e.g., public const string GarageDatabase = "garage-db")
```
