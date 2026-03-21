# Bootstrapping Pattern Comparison: Vehicle vs Location

**Purpose**: Show how Location service follows Vehicle's established bootstrapping pattern, with appropriate adaptations for a reference data service.

---

## Comparison Matrix

| Aspect | Vehicle | Location | Reason |
|--------|---------|----------|--------|
| **AddServiceDefaults()** | ✅ | ✅ | Standard for all services |
| **AddCommonService()** | ✅ | ✅ | Standard for all services |
| **PostgreSQL Database** | ✅ | ✅ | Both use PostgreSQL |
| **Hangfire Jobs** | ✅ (Reminder jobs) | ❌ | Location is read-only, no jobs needed |
| **HTTP Client** | ✅ (Identity service) | ❌ | Location is self-contained, no inter-service calls |
| **UnitOfWork** | ✅ | ✅ | Both use repository pattern |
| **Service Registration** | ✅ (13 services) | ✅ (4 services) | Proportional to entity count |
| **FluentValidation** | ✅ | ❌ | Location is GET-only, no input validation needed |
| **MapOpenApi()** | ✅ (dev) | ✅ (dev) | Swagger for both |
| **RequireRateLimiting()** | ✅ | ✅ | Both protect against abuse |
| **MapXxxApi()** | ✅ (11 endpoints) | ✅ (2 API groups) | Proportional to functionality |

---

## Side-by-Side Comparison

### AddApplicationServices() Method

#### Vehicle Service
```csharp
public static IHostApplicationBuilder AddApplicationServices(
    this IHostApplicationBuilder builder)
{
    // 1. Foundation
    builder.AddServiceDefaults();
    builder.AddCommonService();

    // 2. Database setup
    builder.AddPostgresDatabase<VehicleDbContext>(Const.VehicleDatabase);

    // 3. Hangfire (background jobs - Vehicle-specific)
    var connectionString = builder.Configuration.GetConnectionString(Const.VehicleDatabase)
        ?? throw new InvalidOperationException(...);
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
    builder.Services.AddHangfireServer();

    // 4. HTTP clients (inter-service communication - Vehicle-specific)
    builder.Services.AddScoped<ForwardAuthorizationHandler>();
    builder.Services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
    {
        var baseUrl = builder.Configuration["Identity:BaseUrl"]
            ?? builder.Configuration["Services:Identity:BaseUrl"]
            ?? "https://localhost:8001";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddHttpMessageHandler<ForwardAuthorizationHandler>();

    // 5. Background jobs (Vehicle-specific)
    builder.Services.AddScoped<OdometerReminderJob>();
    builder.Services.AddScoped<MaintenanceReminderJob>();

    // 6. UnitOfWork (common pattern)
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // 7. Services (Vehicle has 13 services)
    builder.Services.AddScoped<ITypeService, TypeService>();
    builder.Services.AddScoped<IBrandService, BrandService>();
    builder.Services.AddScoped<IModelService, ModelService>();
    // ... 10 more services ...

    // 8. Validators (Vehicle has input validation)
    builder.Services.AddValidatorsFromAssemblyContaining<UserVehicleRequestValidator>();

    return builder;
}
```

#### Location Service
```csharp
public static IHostApplicationBuilder AddApplicationServices(
    this IHostApplicationBuilder builder)
{
    // 1. Foundation
    builder.AddServiceDefaults();
    builder.AddCommonService();

    // 2. Database setup
    const string locationDb = "location";
    builder.AddPostgresDatabase<LocationDbContext>(locationDb);

    // 3. Caching (Location-specific for static data)
    var serviceName = "location-service";
    builder.AddServiceRedis(serviceName);

    // 4. UnitOfWork (common pattern)
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // 5. Services (Location has 4 services - read-only)
    builder.Services.AddScoped<IProvinceService, ProvinceService>();
    builder.Services.AddScoped<IWardService, WardService>();
    builder.Services.AddScoped<IAdministrativeUnitService, AdministrativeUnitService>();
    builder.Services.AddScoped<IAdministrativeRegionService, AdministrativeRegionService>();

    // Note: No validators (no input data), no Hangfire, no HTTP clients

    return builder;
}
```

### Key Differences Explained

| Vehicle Feature | Location | Why? |
|-----------------|----------|------|
| **Hangfire Configuration** | ❌ Not needed | Location is read-only; no scheduled maintenance tasks |
| **HTTP Clients to other services** | ❌ Not needed | Location is self-contained; doesn't depend on other services |
| **FluentValidation Registration** | ❌ Not needed | All endpoints are GET (read-only); no input validation needed |
| **Redis Cache Registration** | ✅ Added | Location data is frequently accessed; 24h caching reduces DB load |

---

### UseApplicationServices() Method

#### Vehicle Service
```csharp
public static WebApplication UseApplicationServices(this WebApplication app)
{
    // 1. Standard middleware
    app.MapDefaultEndpoints();
    app.UseCommonService();

    // 2. Hangfire dashboard (dev only, Vehicle-specific)
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = Array.Empty<IDashboardAuthorizationFilter>()
        });
    }

    // 3. Schedule recurring jobs (Vehicle-specific)
    RecurringJob.AddOrUpdate<OdometerReminderJob>(
        "odometer-reminder",
        x => x.ExecuteAsync(CancellationToken.None),
        "0 0 * * *");
    RecurringJob.AddOrUpdate<MaintenanceReminderJob>(
        "maintenance-reminder-Critical",
        x => x.ExecuteAsync(CancellationToken.None),
        "0 0 * * *");

    // 4. OpenAPI/Swagger (dev only)
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // 5. HTTPS
    app.UseHttpsRedirection();

    // 6. API endpoints (Vehicle has 11)
    app.MapBrandApi();
    app.MapTypeApi();
    app.MapModelApi();
    app.MapModelImageApi();
    app.MapUserVehicleApi();
    app.MapOdometerHistoryApi();
    app.MapDefaultMaintenanceScheduleApi();
    app.MapMaintenanceRecordApi();
    app.MapPartCategoryApi();
    app.MapPartProductApi();
    app.MapInternalVehicleApi();

    return app;
}
```

#### Location Service
```csharp
public static WebApplication UseApplicationServices(this WebApplication app)
{
    // 1. Standard middleware
    app.MapDefaultEndpoints();
    app.UseCommonService();

    // 2. OpenAPI/Swagger (dev only)
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // 3. HTTPS
    app.UseHttpsRedirection();

    // 4. API endpoints (Location has 2 groups)
    app.MapLocationApi();
    app.MapInternalLocationApi();

    // Note: No Hangfire dashboard, no job scheduling

    return app;
}
```

### Key Differences Explained

| Vehicle Feature | Location | Why? |
|-----------------|----------|------|
| **Hangfire Dashboard** | ❌ Not needed | No background jobs in Location |
| **RecurringJob.AddOrUpdate()** | ❌ Not needed | No scheduled tasks in Location |
| **11 MapXxxApi() calls** | ✅ 2 calls | Location has fewer, simpler endpoints |

---

## Architecture Pattern (Both Services)

### Layer Structure (Same for Both)
```
Domain Layer
    ↓
Application Layer
    ↓
Infrastructure Layer
    ↓
Host/API Layer
```

### DI Registration Order (Both Follow Same Pattern)

**Step 1: Foundation**
```csharp
builder.AddServiceDefaults();
builder.AddCommonService();
```

**Step 2: Data Access**
```csharp
builder.AddPostgresDatabase<DbContext>(connectionName);
// Vehicle: Hangfire + HTTP clients
// Location: Redis cache
```

**Step 3: Core Services**
```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**Step 4: Domain Services**
```csharp
builder.Services.AddScoped<IXxxService, XxxService>();
```

**Step 5: Cross-Cutting Concerns**
```csharp
// Vehicle: Validators, HTTP handlers, background jobs
// Location: None (read-only service)
```

### Middleware Pipeline Order (Both Follow Same Pattern)

**1. Infrastructure**
```csharp
app.MapDefaultEndpoints();   // Health checks
app.UseCommonService();       // Common middleware
```

**2. Development Tools**
```csharp
if (app.Environment.IsDevelopment())
{
    // Vehicle: Hangfire dashboard
    // Location: (None - no background jobs)
    app.MapOpenApi();          // Swagger
}
```

**3. Scheduled Work**
```csharp
// Vehicle: RecurringJob.AddOrUpdate() calls
// Location: (None - read-only)
```

**4. Security**
```csharp
app.UseHttpsRedirection();
```

**5. API Routes**
```csharp
app.MapBrandApi();      // Vehicle: 11 endpoint groups
app.MapLocationApi();   // Location: 2 endpoint groups
```

---

## Service Registration Pattern (Identical)

### Vehicle Services (13 total)
```csharp
builder.Services.AddScoped<ITypeService, TypeService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<IVariantService, VariantService>();
builder.Services.AddScoped<IUserVehicleService, UserVehicleService>();
builder.Services.AddScoped<IMaintenanceReminderService, MaintenanceReminderService>();
builder.Services.AddScoped<IOdometerHistoryService, OdometerHistoryService>();
builder.Services.AddScoped<IPartTrackingService, PartTrackingService>();
builder.Services.AddScoped<IDefaultScheduleService, DefaultScheduleService>();
builder.Services.AddScoped<IPartCategoryService, PartCategoryService>();
builder.Services.AddScoped<IPartProductService, PartProductService>();
builder.Services.AddScoped<IMaintenanceRecordService, MaintenanceRecordService>();
```

### Location Services (4 total)
```csharp
builder.Services.AddScoped<IProvinceService, ProvinceService>();
builder.Services.AddScoped<IWardService, WardService>();
builder.Services.AddScoped<IAdministrativeUnitService, AdministrativeUnitService>();
builder.Services.AddScoped<IAdministrativeRegionService, AdministrativeRegionService>();
```

**Pattern**: `AddScoped<IInterface, Implementation>()` — identical for both.

---

## Summary: Location Correctly Follows Verendar Pattern

✅ **Identical Aspects**:
- Foundation setup (AddServiceDefaults, AddCommonService)
- Database configuration
- UnitOfWork pattern
- Service registration pattern
- Middleware pipeline structure
- API endpoint mapping pattern
- Development-only middleware

✅ **Appropriate Differences**:
- **Location omits** Hangfire (no background jobs)
- **Location omits** HTTP clients (no inter-service dependencies)
- **Location omits** Validators (GET-only, no input validation)
- **Location adds** Redis cache (for 24h caching of static data)

✅ **Conclusion**: Location service follows the exact bootstrapping pattern established by Vehicle service, with only the differences needed for its specific use case (read-only reference data service).

---

## Reference

- **Vehicle Service**: `Vehicle/Verendar.Vehicle/Bootstrapping/ApplicationServiceExtensions.cs`
- **Location Service**: `Location/Verendar.Location/Bootstrapping/ApplicationServiceExtensions.cs`
- **Pattern**: `.claude/patterns/API_BUILDING_PATTERN.md`
