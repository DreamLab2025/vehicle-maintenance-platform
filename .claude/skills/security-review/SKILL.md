---
name: security-review
description: Use this skill when adding authentication, handling user input, working with secrets, creating API endpoints, or implementing payment/sensitive features. Provides security checklist with .NET/Verendar patterns. Activate proactively when the user mentions auth, JWT, secrets, file uploads, payment (VNPay), or sensitive data in the Verendar codebase.
---

# Security Review — .NET / Verendar

## 1. Secrets Management

Never put secrets in `appsettings.json`. Use **User Secrets** in dev, environment variables in prod.

```csharp
// Register
builder.Services.Configure<JwtBearerConfigurationOptions>(
    builder.Configuration.GetSection("Jwt"));

// Inject — never inject IConfiguration directly into services
public class MyService(IOptions<JwtBearerConfigurationOptions> options) { }
```

```bash
dotnet user-secrets set "Jwt:SecretKey" "your-key" --project Verendar.Identity
```

**Verify**: No hardcoded keys/passwords in source; `.env` files in `.gitignore`; prod secrets in environment.

---

## 2. Input Validation

Use `ValidationEndpointFilter` at the endpoint — do not re-validate inside handlers.

```csharp
group.MapPost("/bookings", CreateHandler)
     .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateBookingRequest>());

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.VehicleId).NotEmpty();
    }
}
```

**Verify**: All request DTOs have a validator; file uploads check size + MIME type + extension.

---

## 3. SQL Injection

EF Core LINQ is parameterized by default. Dangerous only with raw SQL.

```csharp
// SAFE — always use LINQ
var user = await db.Users.Where(u => u.Email == email).FirstOrDefaultAsync(ct);

// SAFE — raw SQL with parameters
await db.Users.FromSqlRaw("SELECT * FROM users WHERE email = {0}", email).ToListAsync(ct);

// NEVER — string interpolation in raw SQL
// db.Users.FromSqlRaw($"SELECT * WHERE email = '{email}'")
```

---

## 4. Authentication & Authorization

JWT auth is configured by `AddCommonService()`. Internal endpoints require `Role = "Service"`.

```csharp
// Public endpoint — require authenticated user
group.MapGet("/bookings/{id}", GetHandler).RequireAuthorization();

// Admin-only
group.MapDelete("/users/{id}", DeleteHandler)
     .RequireAuthorization(p => p.RequireRole("Admin"));

// Extract caller identity in handler
var callerId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

// Internal endpoint — service-to-service only
internalGroup.MapPost("/{userId}/roles", AssignRoleHandler)
             .RequireAuthorization(p => p.RequireRole("Service"));
```

**Verify**: Every endpoint has `RequireAuthorization()` or is explicitly public; ownership checked before mutations.

---

## 5. Error Responses — RFC 7807

Never expose stack traces or exception messages to clients.

```csharp
// CORRECT
return Results.Problem(
    detail: "An error occurred processing your request.",
    statusCode: 500);

// WRONG — never do this
return Results.Problem(detail: ex.ToString(), statusCode: 500);
```

---

## 6. Sensitive Data in Logs

```csharp
// WRONG
logger.LogInformation("Login: email={Email} password={Password}", email, password);

// CORRECT — log IDs, not credentials
logger.LogInformation("Login attempt: userId={UserId}", userId);

// Mask PII in logs
logger.LogInformation("OTP sent to {Phone}", phone[..3] + "****" + phone[^2..]);
```

---

## 7. Rate Limiting

Rate limiting is configured globally by `AddCommonService()`. No per-endpoint config needed for standard limits.

**Verify**: Payment and auth endpoints have stricter limits; no unbounded expensive operations.

---

## 8. Dependency Security

```bash
dotnet list package --vulnerable   # check for known CVEs
dotnet outdated                    # check for updates
```

---

## 9. Blockchain (Solana)

See [`references/blockchain.md`](references/blockchain.md) if working with wallet auth or on-chain transactions.

---

## Pre-Deployment Checklist

- [ ] No hardcoded secrets; all in User Secrets / env
- [ ] All request DTOs have FluentValidation validators
- [ ] EF Core queries use LINQ (not raw SQL interpolation)
- [ ] Every endpoint has authorization attribute or is explicitly public
- [ ] No stack traces in error responses (RFC 7807 Problem Details)
- [ ] No credentials or PII in logs
- [ ] Rate limiting active on auth and payment endpoints
- [ ] File uploads validate size, MIME type, and extension
- [ ] HTTPS enforced; `Secure` + `HttpOnly` + `SameSite=Strict` on cookies
- [ ] `dotnet list package --vulnerable` returns clean
