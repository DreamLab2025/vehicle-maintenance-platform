# Service Communication — HTTP & MassTransit

To choose and implement the right inter-service communication channel in Verendar: synchronous HTTP for data fetching, MassTransit/RabbitMQ for async event choreography.

---

## When to use which

| Need                                                | Channel                                   |
| --------------------------------------------------- | ----------------------------------------- |
| Caller needs a response to complete its own request | HTTP (`/api/internal/...`)                |
| "Something happened, others should react"           | MassTransit publish                       |
| Payment flow: Garage → Payment                      | HTTP initiate, MassTransit consume result |
| One event, multiple consumers                       | MassTransit                               |

---

## HTTP Client (Synchronous)

### File layout

```
Verendar.{Service}.Application/
└── Clients/
    └── I{Target}Client.cs          ← interface (domain contract, no HTTP knowledge)

Verendar.{Service}.Infrastructure/
└── Clients/
    └── {Target}HttpClient.cs       ← implementation

Verendar.{Service}/
└── Bootstrapping/
    └── ClientExtensions.cs         ← DI registration + Polly
```

### Interface (Application layer)

Return domain types, never `HttpResponseMessage`.

```csharp
// Application/Clients/IPaymentClient.cs
public interface IPaymentClient
{
    Task<PaymentInitiateResult> InitiateAsync(
        Guid bookingId, Money amount, string returnUrl, CancellationToken ct = default);
}

public record PaymentInitiateResult(bool Success, Guid? PaymentId, string? PaymentUrl, string? ErrorMessage);
```

### Simple client (Infrastructure layer)

Use when the target endpoint does NOT wrap in `ApiResponse<T>`.

```csharp
// Infrastructure/Clients/PaymentHttpClient.cs
public class PaymentHttpClient(HttpClient httpClient, ILogger<PaymentHttpClient> logger) : IPaymentClient
{
    public async Task<PaymentInitiateResult> InitiateAsync(
        Guid bookingId, Money amount, string returnUrl, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "/api/internal/payments/initiate",
                new { BookingId = bookingId, Amount = amount.Amount, ReturnUrl = returnUrl }, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning("Payment initiation failed for booking {BookingId}: {Error}", bookingId, error);
                return new PaymentInitiateResult(false, null, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<PaymentInitiateResponse>(cancellationToken: ct);
            return new PaymentInitiateResult(true, result!.PaymentId, result.PaymentUrl, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initiating payment for booking {BookingId}", bookingId);
            return new PaymentInitiateResult(false, null, null, ex.Message);
        }
    }

    private sealed record PaymentInitiateResponse(Guid PaymentId, string PaymentUrl);
}
```

### Client with BaseServiceClient (when target returns `ApiResponse<T>`)

```csharp
// Infrastructure/Clients/VehicleServiceClient.cs
public class VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
    : BaseServiceClient<VehicleServiceClient>(httpClient, logger), IVehicleServiceClient
{
    protected override string ServiceName => "Vehicle Service";

    public Task<ApiResponse<UserVehicleResponse>> GetUserVehicleByIdAsync(
        Guid userVehicleId, CancellationToken ct = default) =>
        GetAsync<UserVehicleResponse>(
            $"/api/internal/vehicles/user-vehicles/{userVehicleId}",
            $"user vehicle {userVehicleId}", ct);
}
```

`BaseServiceClient` handles: request/response logging, `BrokenCircuitException` → `FailureResponse`, all exceptions → `FailureResponse`.

### DI registration

```csharp
// Bootstrapping/ClientExtensions.cs
public static IHostApplicationBuilder AddClients(this IHostApplicationBuilder builder)
{
    builder.Services
        .AddHttpClient<IPaymentClient, PaymentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["PaymentService:BaseUrl"]
                ?? builder.Configuration["Services:Payment:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<ForwardAuthorizationHandler>()  // forward JWT token
        .AddPolicyHandler(ResiliencePolicies.Standard());      // retry + circuit breaker

    return builder;
}
```

`ForwardAuthorizationHandler` propagates the incoming `Authorization: Bearer ...` header to downstream services. BaseAddress comes from configuration — never hardcode URLs.

---

## MassTransit / RabbitMQ (Async Events)

### File layout

```
Verendar.{Service}.Contracts/
└── Events/
    └── {EventName}Event.cs         ← shared contract; other services reference this project

Verendar.{Service}.Application/
└── Consumers/
    └── {EventName}Consumer.cs      ← handles event from another service
```

### Event contract

```csharp
// Verendar.Garage.Contracts/Events/BookingCompletedEvent.cs
public class BookingCompletedEvent : BaseEvent
{
    public override string EventType => "garage.booking.completed.v1";  // {service}.{aggregate}.{verb}.v{n}

    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid UserVehicleId { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

`BaseEvent` (from `Verendar.Common`) provides: `EventId`, `OccurredAt`, and abstract `EventType`.

### Publishing

Persist first, then publish. If publish fails, log a warning — do not rollback.

```csharp
public class BookingService(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint,
    ILogger<BookingService> logger) : IBookingService
{
    public async Task<ApiResponse<BookingResponse>> CompleteBookingAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking is null) return ApiResponse<BookingResponse>.NotFoundResponse("Không tìm thấy booking");

        booking.Status = BookingStatus.Completed;
        booking.CompletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);   // persist FIRST

        try
        {
            await _publishEndpoint.Publish(new BookingCompletedEvent
            {
                BookingId = booking.Id, UserId = booking.UserId,
                UserVehicleId = booking.UserVehicleId, CompletedAt = booking.CompletedAt!.Value,
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish BookingCompletedEvent {BookingId}", bookingId);
            // do not rethrow — DB is committed, event can be replayed if needed
        }

        return ApiResponse<BookingResponse>.SuccessResponse(booking.ToResponse(), "Hoàn thành booking");
    }
}
```

### Consumer

```csharp
// Application/Consumers/UserRegisteredConsumer.cs
public class UserRegisteredConsumer(
    ILogger<UserRegisteredConsumer> logger,
    IUnitOfWork unitOfWork,
    IEmailNotificationService emailService) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Processing {EventType} MessageId:{MessageId}", msg.EventType, context.MessageId);

        // validate before persisting
        if (string.IsNullOrEmpty(msg.FullName)) { logger.LogWarning("Invalid message, skipping"); return; }

        await unitOfWork.NotificationPreferences.AddAsync(msg.ToPreferenceEntity());
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        await emailService.SendWelcomeEmailAsync(msg, context.CancellationToken);
    }
}
```

**Consumer rules:**

- Do not throw out of `Consume()` unless intentionally triggering retry/dead-letter.
- Validate message data before persisting.
- Use `context.MessageId` for correlation in logs.

### Registration

```csharp
// In AddApplicationServices:
builder.AddRabbitMqEventBus(configurator =>
{
    configurator.AddConsumer<UserRegisteredConsumer>();
    configurator.AddConsumer<BookingCompletedConsumer>();
    // add new consumers here
});
```

`AddRabbitMqEventBus` is a `Verendar.Common` extension that wraps `AddMassTransit` with standard RabbitMQ configuration.
