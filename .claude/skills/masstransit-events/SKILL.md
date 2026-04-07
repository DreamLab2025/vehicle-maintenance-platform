---
name: masstransit-events
description: MassTransit + RabbitMQ event-driven patterns for the Verendar project — publishing events, writing consumers, event contract design, and inter-service async communication. Use this skill whenever publishing an event, creating a consumer, designing event contracts, implementing async inter-service communication, or troubleshooting MassTransit in Verendar. Activate proactively when the user mentions events, consumers, MassTransit, RabbitMQ, pub/sub, IPublishEndpoint, ConsumeContext, BaseEvent, or cross-service notifications.
---

# MassTransit + RabbitMQ Patterns — Verendar

## Event Contract Design

All events **must** extend `BaseEvent` from `Verendar.Common`.

```csharp
// BaseEvent (Verendar.Common/Contracts/BaseEvent.cs)
public abstract class BaseEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
```

**Event naming convention**: `{service}.{entity}.{action}.v{version}`

```csharp
// Verendar.Garage.Contracts/Events/BookingCreatedEvent.cs
public class BookingCreatedEvent : BaseEvent
{
    public override string EventType => "garage.booking.created.v1";

    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid GarageId { get; set; }
    public Guid GarageBranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime ScheduledAt { get; set; }

    // Include denormalized data consumers will need (avoids extra queries)
    public Guid OwnerUserId { get; set; }
    public List<Guid> ManagerUserIds { get; set; } = [];
}
```

**Where contracts live**: `Verendar.{Service}.Contracts/Events/` — a separate project referenced by consuming services. Never put event contracts in Application or Domain.

---

## Publishing Events

Inject `IPublishEndpoint` in your application service:

```csharp
public class BookingService(
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IBookingService
{
    public async Task<ApiResponse<BookingResponse>> CreateAsync(
        CreateBookingRequest request, CancellationToken ct = default)
    {
        var booking = request.ToEntity();
        await unitOfWork.Bookings.AddAsync(booking, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Publish after persisting — ensures event only fires if save succeeded
        await publishEndpoint.Publish(new BookingCreatedEvent
        {
            BookingId = booking.Id,
            UserId    = booking.UserId,
            GarageId  = booking.GarageId,
            // ... fill all fields
        }, ct);

        return ApiResponse<BookingResponse>.Success(booking.ToResponse());
    }
}
```

**Rule**: Always publish *after* `SaveChangesAsync`. Never publish inside a transaction that might roll back.

---

## Writing Consumers

Consumers are **auto-discovered** — no manual registration needed. MassTransit scans all assemblies referenced from the entry assembly for `IConsumer<T>` implementations.

```csharp
// Verendar.{Service}.Application/Consumers/BookingCreatedConsumer.cs
public class BookingCreatedConsumer(
    ILogger<BookingCreatedConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<BookingCreatedEvent>
{
    public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var msg = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        logger.LogDebug("Processing {EventType} — MessageId: {MessageId}, BookingId: {BookingId}",
            msg.EventType, messageId, msg.BookingId);

        try
        {
            // Business logic here
            await DoWorkAsync(msg, context.CancellationToken);

            logger.LogInformation("{EventType} processed — MessageId: {MessageId}",
                msg.EventType, messageId);
        }
        catch (Exception ex)
        {
            // Let MassTransit retry (exponential: 5×, 2–30s)
            // Only catch to log — rethrow so retry kicks in
            logger.LogError(ex, "Error processing {EventType} — MessageId: {MessageId}",
                msg.EventType, messageId);
            throw;
        }
    }
}
```

**Idempotency**: Consumers can be called multiple times due to retries. Make business logic idempotent — check for existing records before inserting, use unique constraints as a safety net.

```csharp
// Idempotent insert — catch unique constraint instead of pre-checking
try
{
    await unitOfWork.Records.AddAsync(new Record { ... });
    await unitOfWork.SaveChangesAsync(ct);
}
catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique") == true)
{
    logger.LogWarning("Duplicate event skipped — already processed");
}
```

---

## Consumer Chaining (Publishing from Consumers)

A consumer can publish new events for downstream services:

```csharp
public class UserRegisteredEventConsumer(
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    ILogger<UserRegisteredEventConsumer> logger) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var msg = context.Message;

        if (string.IsNullOrWhiteSpace(msg.ReferralCode))
            return;

        var garage = await unitOfWork.Garages.FindOneAsync(
            g => g.ReferralCode == msg.ReferralCode);

        if (garage is null)
        {
            logger.LogWarning("Referral code not found: {Code}", msg.ReferralCode);
            return;
        }

        await unitOfWork.Referrals.AddAsync(new GarageReferral
        {
            GarageId       = garage.Id,
            ReferredUserId = msg.UserId,
            ReferralCode   = msg.ReferralCode,
            ReferredAt     = msg.RegistrationDate,
        });
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        // Publish downstream event
        await publishEndpoint.Publish(new GarageReferralRecordedEvent
        {
            GarageId       = garage.Id,
            GarageOwnerId  = garage.OwnerId,
            ReferredUserId = msg.UserId,
        }, context.CancellationToken);
    }
}
```

---

## Retry Policy

Configured globally in `Verendar.Common/Bootstrapping/CommonApplicationServiceExtensions.cs`. No per-consumer configuration needed.

```csharp
cfg.UseMessageRetry(retry =>
{
    retry.Exponential(
        retryLimit:    5,
        minInterval:   TimeSpan.FromSeconds(2),
        maxInterval:   TimeSpan.FromSeconds(30),
        intervalDelta: TimeSpan.FromSeconds(5));
});
```

Failed messages retry 5 times with exponential backoff (2s → 7s → 12s → 17s → 22s). After exhausting retries, the message moves to the error queue.

---

## Adding a New Event — Step-by-Step

1. **Create event contract** in `Verendar.{SourceService}.Contracts/Events/`:
   ```csharp
   public class MyNewEvent : BaseEvent
   {
       public override string EventType => "myservice.entity.action.v1";
       public Guid EntityId { get; set; }
       // Include all data consumers will need — minimize extra queries
   }
   ```

2. **Reference the Contracts project** from the consuming service's Application project (`.csproj`):
   ```xml
   <ProjectReference Include="..\..\SourceService\Verendar.SourceService.Contracts\Verendar.SourceService.Contracts.csproj" />
   ```

3. **Publish** in the source service after persisting:
   ```csharp
   await publishEndpoint.Publish(new MyNewEvent { EntityId = entity.Id }, ct);
   ```

4. **Create consumer** in the consuming service's Application layer:
   ```csharp
   public class MyNewEventConsumer(IUnitOfWork unitOfWork, ILogger<MyNewEventConsumer> logger)
       : IConsumer<MyNewEvent>
   {
       public async Task Consume(ConsumeContext<MyNewEvent> context) { ... }
   }
   ```

5. **No registration needed** — auto-discovery handles it.

---

## Event vs. Sync HTTP — When to Use Which

| Scenario | Use |
|----------|-----|
| Trigger side effects in other services (notifications, referrals, AI re-analysis) | **Event (MassTransit)** |
| Need a response value to continue the current request | **Sync HTTP client** |
| Payment operations | **Sync HTTP** (`IPaymentClient`) + consume `PaymentSucceededEvent` |
| Fire-and-forget after a domain action | **Event (MassTransit)** |

---

## Logging Conventions in Consumers

```csharp
// Entry (Debug — noisy, only useful for troubleshooting)
logger.LogDebug("Processing {EventType} — MessageId: {MessageId}, EntityId: {EntityId}",
    msg.EventType, messageId, msg.EntityId);

// Success (Information)
logger.LogInformation("{EventType} processed — MessageId: {MessageId}", msg.EventType, messageId);

// Expected skip (Warning — visible without noise)
logger.LogWarning("{EventType} skipped — reason: {Reason}", msg.EventType, reason);

// Unexpected error (Error)
logger.LogError(ex, "Error processing {EventType} — MessageId: {MessageId}", msg.EventType, messageId);
```

Always log `MessageId` so you can trace a message through retries in Seq.
