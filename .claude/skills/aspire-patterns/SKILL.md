---
name: aspire-patterns
description: .NET Aspire 9.5 patterns for the Verendar project — service bootstrap, service discovery, AppHost orchestration, health checks, and internal service authentication. Use this skill whenever adding a new service, configuring Aspire infrastructure, wiring up service discovery, setting up typed HTTP clients, or troubleshooting inter-service communication in Verendar. Activate proactively when the user mentions Aspire, AppHost, AddServiceDefaults, service bootstrap, or any of the Verendar services (Identity, Garage, Vehicle, Notification, AI, Payment, Location, Media).
---

# Aspire 9.5 Patterns — Verendar

## Service Bootstrap Order

Every Host project follows this exact sequence. Do not deviate.

### ApplicationServiceExtensions.cs (`Bootstrapping/`)

```csharp
public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
{
    // 1. Mandatory — OpenTelemetry, health checks, service discovery
    builder.AddServiceDefaults();

    // 2. Mandatory — CORS, rate limiting, MassTransit, JWT auth, IServiceTokenProvider
    builder.AddCommonService();

    // 3. Always — EF Core + Npgsql
    builder.AddPostgresDatabase<MyDbContext>(Const.MyServiceDatabase);

    // 4. Only if service uses Redis
    builder.AddServiceRedis(nameof(MyService), connectionName: Const.Redis);

    // 5. DI registrations (scoped services, validators, HTTP clients...)
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IMyService, MyService>();
    builder.Services.AddValidatorsFromAssemblyContaining<MyRequestValidator>();

    return builder;
}

public static WebApplication UseApplicationServices(this WebApplication app)
{
    app.MapDefaultEndpoints();          // /health, /alive
    app.UseHttpsRedirection();
    app.UseCommonService();             // Auth middleware, CORS, rate limiting
    app.MapOpenApi();
    app.MapMyModuleApi();               // Route group mappings
    return app;
}
```

### Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.MigrateAsync();           // Apply EF Core migrations
    await app.SeedAsync();              // Seed dev data (dev only)
}

app.UseApplicationServices();
app.Run();
```

---

## ServiceDefaults — What It Provides

`AddServiceDefaults()` (in `Verendar.ServiceDefaults`) configures:
- **Serilog** structured logging with OpenTelemetry export
- **Health endpoints**: `/health` (full) and `/alive` (liveness)
- **Service discovery**: `IServiceDiscovery` registered globally
- **HTTP client resilience**: standard retry/timeout policies on all `HttpClient` instances

---

## AppHost Orchestration

Services are registered in `App/Verendar.AppHost/Extensions/ExternalServiceRegistrationExtensions.cs`:

```csharp
// Infrastructure resources
var postgres = builder.AddPostgres("postgres").WithDataVolume().WithPgWeb();
var rabbitMq = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin();
var redis    = builder.AddRedis("redis");
var seq      = builder.AddSeq("seq").ExcludeFromManifest();

// Per-service database
var identityDb = postgres.AddDatabase(Const.IdentityDatabase);

// Service registration
var identityService = builder.AddProject<Projects.Verendar_Identity>("identity-service")
    .WithReference(identityDb)
    .WithReference(rabbitMq)
    .WithReference(redis)
    .WithReference(seq)
    .WaitFor(identityDb);
```

When adding a **new service**:
1. Add a database: `var myDb = postgres.AddDatabase(Const.MyServiceDatabase);`
2. Add a constant in `Verendar.Common/Shared/Const.cs`: `public const string MyServiceDatabase = "my-service-db";`
3. Register the project with its dependencies

---

## Service Discovery (Typed HTTP Clients)

Services communicate synchronously via typed HTTP clients. Register in the consuming service's `Bootstrapping/ApplicationServiceExtensions.cs`:

```csharp
// Without internal auth (public-facing internal endpoint)
builder.Services.AddHttpClient<ILocationClient, LocationHttpClient>(client =>
{
    var baseAddress = builder.Configuration["Services:Location:BaseUrl"];
    client.BaseAddress = new Uri(string.IsNullOrEmpty(baseAddress)
        ? "https+http://location-service"      // Aspire service discovery name
        : baseAddress);
}).AddServiceDiscovery();

// With internal auth (uses IServiceTokenProvider bearer token)
builder.Services.AddHttpClient<IIdentityClient, IdentityHttpClient>(client =>
{
    var baseAddress = builder.Configuration["Services:Identity:BaseUrl"];
    client.BaseAddress = new Uri(string.IsNullOrEmpty(baseAddress)
        ? "https+http://identity-service"
        : baseAddress);
}).AddServiceDiscovery();
```

- Service discovery name matches the AppHost registration key (e.g., `"identity-service"`)
- Override via `Configuration["Services:{Name}:BaseUrl"]` for local/staging without Aspire

---

## Internal Service Authentication

Internal endpoints (`/api/internal/...`) authenticate with a short-lived JWT generated by `IServiceTokenProvider`. It is registered automatically by `AddCommonService()`.

```csharp
// In the HTTP client implementation (Infrastructure layer)
public class IdentityHttpClient(
    HttpClient httpClient,
    IServiceTokenProvider serviceTokenProvider,
    ILogger<IdentityHttpClient> logger) : IIdentityClient
{
    public async Task<bool> AssignRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"/api/internal/users/{userId}/roles");
        request.Content = JsonContent.Create(new { Role = role });
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", serviceTokenProvider.GenerateServiceToken());

        var response = await httpClient.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }
}
```

The token claims `Role = "Service"` and expires in 5 minutes — just right for a single call.

---

## Typed HTTP Client — Full Pattern

**Interface** (Application layer — `Clients/IMyServiceClient.cs`):
```csharp
public interface IMyServiceClient
{
    Task<MyResponseDto?> GetSomethingAsync(Guid id, CancellationToken ct = default);
}
```

**Implementation** (Infrastructure layer — `Clients/MyServiceHttpClient.cs`):
```csharp
public class MyServiceHttpClient(
    HttpClient httpClient,
    ILogger<MyServiceHttpClient> logger) : IMyServiceClient
{
    public async Task<MyResponseDto?> GetSomethingAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<MyResponseDto>(
                $"/api/internal/something/{id}", ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling MyService for id {Id}", id);
            return null;
        }
    }
}
```

---

## Health Checks

`MapDefaultEndpoints()` (from `AddServiceDefaults`) automatically exposes:
- `GET /health` — full health check (DB, external dependencies)
- `GET /alive` — lightweight liveness probe

No additional configuration needed. EF Core and Redis health checks are registered by `AddPostgresDatabase` and `AddServiceRedis`.

---

## Constants Reference

`Verendar.Common/Shared/Const.cs`:

```csharp
public class Const
{
    public const string RabbitMQ        = "rabbitmq";
    public const string Redis           = "redis";
    public const string IdentityDatabase    = "identity-db";
    public const string VehicleDatabase     = "vehicle-db";
    public const string GarageDatabase      = "garage-db";
    public const string NotificationDatabase = "notification-db";
    public const string AiDatabase          = "ai-db";
    public const string LocationDatabase    = "location-db";
    public const string MediaDatabase       = "media-db";
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
}
```

---

## Checklist When Adding a New Service

- [ ] Create 4 projects: `Verendar.{Service}`, `.Application`, `.Domain`, `.Infrastructure`
- [ ] Add `Verendar.{Service}.Contracts` project for event contracts
- [ ] Add database constant in `Const.cs`
- [ ] Create `Bootstrapping/ApplicationServiceExtensions.cs` (AddApplicationServices + UseApplicationServices)
- [ ] Register service in AppHost with correct resource references
- [ ] Add service to YARP route config if externally accessible
- [ ] Put secrets in User Secrets — never in `appsettings.json`
