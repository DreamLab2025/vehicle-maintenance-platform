# Security — .NET / Verendar-Specific Patterns

## Secrets Management in .NET

**NEVER** put secrets in `appsettings.json` — this is an explicit constraint in Verendar.

```bash
# Store secrets in User Secrets (dev)
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key" --project Verendar.Identity
dotnet user-secrets set "ConnectionStrings:identity-db" "Host=..." --project Verendar.Identity
```

```csharp
// Bind safely via IOptions<T> — never read Configuration["key"] directly in services
builder.Services.Configure<JwtBearerConfigurationOptions>(
    builder.Configuration.GetSection("Jwt"));

// In service — inject IOptions<T>, not IConfiguration
public class MyService(IOptions<JwtBearerConfigurationOptions> options)
{
    private readonly JwtBearerConfigurationOptions _jwt = options.Value;
}
```

## Input Validation via ValidationEndpointFilter

All Verendar endpoints use `ValidationEndpointFilter` — do not duplicate validation in handlers:

```csharp
// In {Module}Apis.cs — attach filter at endpoint level
group.MapPost("/bookings", CreateBookingHandler)
     .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateBookingRequest>());

// Validator (FluentValidation) — business rules here, not in handler
public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.VehicleId).NotEmpty();
    }
}
```

## SQL Injection Prevention — EF Core

EF Core LINQ queries are parameterized by default. Raw SQL needs explicit care:

```csharp
// SAFE — LINQ queries are always parameterized
var user = await dbContext.Users
    .Where(u => u.Email == userEmail)
    .FirstOrDefaultAsync(ct);

// SAFE — FromSqlRaw with parameters
var results = await dbContext.Users
    .FromSqlRaw("SELECT * FROM users WHERE email = {0}", userEmail)
    .ToListAsync(ct);

// DANGEROUS — string interpolation in raw SQL (never do this)
// var results = dbContext.Users.FromSqlRaw($"SELECT * WHERE email = '{userEmail}'");
```

## Authorization in Minimal API

```csharp
// Role-based authorization on endpoint
group.MapDelete("/users/{id}", DeleteUserHandler)
     .RequireAuthorization(policy => policy.RequireRole("Admin"));

// In handler — get caller's identity from HttpContext
private static async Task<IResult> DeleteUserHandler(
    Guid id, HttpContext ctx, IUserService userService, CancellationToken ct)
{
    var callerId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await userService.DeleteAsync(id, callerId, ct);
    return result.IsSuccess ? Results.NoContent() : Results.Problem(result.Error);
}
```

## Error Responses — RFC 7807

Verendar maps errors to RFC 7807 Problem Details — never expose internal details:

```csharp
// CORRECT — generic problem response
return Results.Problem(
    detail: "An error occurred while processing your request.",
    statusCode: 500);

// WRONG — never expose stack trace or exception message to client
return Results.Problem(detail: ex.ToString(), statusCode: 500);
```

## Sensitive Data in Logs

```csharp
// WRONG — logging sensitive data
logger.LogInformation("User login: email={Email}, password={Password}", email, password);

// CORRECT — log identifier only, never credentials
logger.LogInformation("User login attempt: userId={UserId}", userId);

// CORRECT — mask phone numbers etc.
logger.LogInformation("OTP sent to {Phone}", phone[..3] + "****" + phone[^2..]);
```
