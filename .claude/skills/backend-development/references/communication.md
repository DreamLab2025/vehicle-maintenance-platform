# Service Communication Patterns

Verendar dùng hai kênh để các service nói chuyện với nhau:
- **HTTP** — cho các yêu cầu đồng bộ, cần phản hồi ngay (internal API calls).
- **RabbitMQ / MassTransit** — cho sự kiện bất đồng bộ, fire-and-forget hoặc choreography.

---

## HTTP Client (Service-to-Service)

### Khi nào dùng HTTP?
- Service A cần dữ liệu của Service B để hoàn thành request hiện tại (ví dụ: Garage gọi Payment để khởi tạo thanh toán).
- Internal endpoints: route `/api/internal/...`, **không** wrap trong `ApiResponse<T>`, không cần auth token thông thường.

### Cấu trúc thư mục

```
Verendar.{Service}.Application/
└── Clients/
    └── I{Target}Client.cs          ← interface (domain contract)

Verendar.{Service}.Infrastructure/
└── Clients/
    └── {Target}HttpClient.cs       ← implementation

Verendar.{Service}/
└── Bootstrapping/
    └── ClientExtensions.cs         ← DI registration + Polly
```

### 1. Khai báo interface (Application layer)

Interface thuộc Application — chỉ nói "tôi cần gì", không biết HTTP tồn tại.

```csharp
// Garage/Verendar.Garage.Application/Clients/IPaymentClient.cs
public interface IPaymentClient
{
    Task<PaymentInitiateResult> InitiateAsync(
        Guid bookingId,
        Money amount,
        string returnUrl,
        CancellationToken ct = default);

    Task<bool> RefundAsync(
        Guid paymentId,
        Money amount,
        CancellationToken ct = default);
}

// Kết quả trả về là domain type, KHÔNG phải HttpResponseMessage
public record PaymentInitiateResult(
    bool Success,
    Guid? PaymentId,
    string? PaymentUrl,
    string? ErrorMessage);
```

### 2a. Simple client (Infrastructure layer)

Dùng khi endpoint target trả về response đơn giản, không cần `BaseServiceClient`.

```csharp
// Garage/Verendar.Garage.Infrastructure/Clients/PaymentHttpClient.cs
public class PaymentHttpClient(
    HttpClient httpClient,
    ILogger<PaymentHttpClient> logger) : IPaymentClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<PaymentHttpClient> _logger = logger;

    public async Task<PaymentInitiateResult> InitiateAsync(
        Guid bookingId, Money amount, string returnUrl, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/internal/payments/initiate",
                new { BookingId = bookingId, Amount = amount.Amount, Currency = amount.Currency, ReturnUrl = returnUrl },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Payment initiation failed for booking {BookingId}: {Error}", bookingId, error);
                return new PaymentInitiateResult(false, null, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<PaymentInitiateResponse>(cancellationToken: ct);
            return new PaymentInitiateResult(true, result!.PaymentId, result.PaymentUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for booking {BookingId}", bookingId);
            return new PaymentInitiateResult(false, null, null, ex.Message);
        }
    }

    // Private record chỉ dùng để deserialize — không để lộ ra ngoài
    private sealed record PaymentInitiateResponse(Guid PaymentId, string PaymentUrl);
}
```

### 2b. Client kế thừa BaseServiceClient (khi target trả `ApiResponse<T>`)

Khi endpoint target wrap kết quả trong `ApiResponse<T>`, dùng `BaseServiceClient<T>` để tái sử dụng logic try/catch + deserialization.

```csharp
// Ai/Verendar.Ai.Application/Clients/VehicleServiceClient.cs
public class VehicleServiceClient(
    HttpClient httpClient,
    ILogger<VehicleServiceClient> logger)
    : BaseServiceClient<VehicleServiceClient>(httpClient, logger), IVehicleServiceClient
{
    protected override string ServiceName => "Vehicle Service";

    public Task<ApiResponse<VehicleServiceUserVehicleResponse>> GetUserVehicleByIdAsync(
        Guid userVehicleId, CancellationToken ct = default) =>
        GetAsync<VehicleServiceUserVehicleResponse>(
            $"/api/internal/vehicles/user-vehicles/{userVehicleId}",
            $"user vehicle {userVehicleId}",
            ct);
}
```

`BaseServiceClient.GetAsync<T>` xử lý:
- Logging request/response
- `BrokenCircuitException` → `ApiResponse.FailureResponse()`
- Bất kỳ exception nào → `ApiResponse.FailureResponse()`

### 3. Đăng ký DI + Polly resilience

```csharp
// {Service}/Verendar.{Service}/Bootstrapping/ClientExtensions.cs
public static IHostApplicationBuilder AddClients(this IHostApplicationBuilder builder)
{
    builder.Services
        .AddHttpClient<IPaymentClient, PaymentHttpClient>(client =>
        {
            var baseUrl = builder.Configuration["PaymentService:BaseUrl"]
                ?? builder.Configuration["Services:Payment:BaseUrl"];
            client.BaseAddress = new Uri(baseUrl!);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<ForwardAuthorizationHandler>()  // forward JWT
        .AddPolicyHandler(GetResiliencePolicy());

    return builder;
}

private static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy()
{
    var circuitBreaker = HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));

    var retry = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    return Policy.WrapAsync(retry, circuitBreaker);
}
```

**Lưu ý:**
- `ForwardAuthorizationHandler` tự động forward `Authorization: Bearer ...` header từ incoming request.
- Polly: retry 3 lần với exponential backoff, circuit breaker mở sau 5 lỗi liên tiếp trong 30s.
- BaseAddress lấy từ `IConfiguration` — không hardcode URL.

---

## RabbitMQ / MassTransit (Event-Driven)

### Khi nào dùng message queue?
- Sau khi persist thành công, thông báo cho các service khác mà không cần đợi họ xử lý xong.
- Ví dụ: Garage publish `BookingCompletedEvent` → Vehicle cập nhật odometer, Notification gửi email.

### Cấu trúc thư mục

```
Verendar.{Service}.Contracts/       ← NuGet-style shared package (chứa event types)
└── Events/
    └── {EventName}Event.cs

Verendar.{Service}.Application/
└── Consumers/
    └── {EventName}Consumer.cs      ← xử lý event từ service khác

Verendar.{Service}/
└── Bootstrapping/
    └── ApplicationServiceExtensions.cs  ← đăng ký MassTransit + consumers
```

### 1. Định nghĩa Event Contract

Event contract sống trong project `.Contracts` riêng — các service khác reference project này để subscribe.

```csharp
// Garage/Verendar.Garage.Contracts/Events/BookingCompletedEvent.cs
public class BookingCompletedEvent : BaseEvent
{
    public override string EventType => "garage.booking.completed.v1";

    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid UserVehicleId { get; set; }
    public Guid GarageBranchId { get; set; }
    public Guid GarageProductId { get; set; }
    public int? CurrentOdometer { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

`BaseEvent` (từ `Verendar.Common`) cung cấp sẵn: `EventId` (Guid), `OccurredAt` (DateTime.UtcNow), và abstract `EventType`.

**Convention đặt tên EventType:** `{service}.{aggregate}.{verb}.v{version}` — ví dụ `garage.booking.completed.v1`.

### 2. Publish event (từ service)

Persist trước, publish sau. Nếu publish thất bại thì chỉ log warning — không rollback.

```csharp
// Trong service class
public class BookingService(
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    ILogger<BookingService> logger) : IBookingService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<BookingService> _logger = logger;

    public async Task<ApiResponse<BookingResponse>> CompleteBookingAsync(Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking is null)
            return ApiResponse<BookingResponse>.NotFoundResponse("Không tìm thấy booking");

        booking.Status = BookingStatus.Completed;
        booking.CompletedAt = DateTime.UtcNow;

        await _unitOfWork.Bookings.UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync();   // ← persist TRƯỚC

        try
        {
            await _publishEndpoint.Publish(new BookingCompletedEvent
            {
                BookingId      = booking.Id,
                UserId         = booking.UserId,
                UserVehicleId  = booking.UserVehicleId,
                GarageBranchId = booking.GarageBranchId,
                GarageProductId = booking.GarageProductId,
                CompletedAt    = booking.CompletedAt.Value,
            });
        }
        catch (Exception ex)
        {
            // Không rollback — event có thể retry sau nếu cần
            _logger.LogWarning(ex, "Failed to publish BookingCompletedEvent {BookingId}", bookingId);
        }

        return ApiResponse<BookingResponse>.SuccessResponse(booking.ToResponse(), "Hoàn thành booking");
    }
}
```

### 3. Viết Consumer

```csharp
// Notification/Verendar.Notification.Application/Consumers/UserRegisteredConsumer.cs
public class UserRegisteredConsumer(
    ILogger<UserRegisteredConsumer> logger,
    IUnitOfWork unitOfWork,
    IEmailNotificationService emailNotificationService) : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailNotificationService _emailNotificationService = emailNotificationService;

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Processing {EventType} — MessageId: {MessageId}, UserId: {UserId}",
            message.EventType, messageId, message.UserId);

        try
        {
            // 1. Validate trước khi làm gì
            if (string.IsNullOrEmpty(message.FullName) || message.RegistrationDate == DateTime.MinValue)
            {
                _logger.LogWarning("Invalid message — MessageId: {MessageId}", messageId);
                return;   // không throw → message ack'd và không retry
            }

            // 2. Side effects: persist + gọi service
            await _unitOfWork.NotificationPreferences.AddAsync(message.UserRegisteredToPreferenceEntity());
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            await _emailNotificationService.SendWelcomeEmailAsync(message, context.CancellationToken);

            _logger.LogInformation(
                "Processed {EventType} — MessageId: {MessageId}", message.EventType, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing {EventType} — MessageId: {MessageId}", message.EventType, messageId);
            // throw; ← bỏ comment này nếu muốn MassTransit retry / dead-letter
        }
    }
}
```

**Quy tắc consumer:**
- Không bao giờ throw exception ra ngoài `Consume()` trừ khi cố ý muốn retry.
- `context.CancellationToken` để pass vào async calls bên trong.
- Dùng `context.MessageId` để correlation logging — tìm kiếm log dễ hơn.
- Validate message data trước khi persist để tránh lưu dữ liệu rác.

### 4. Đăng ký MassTransit + Consumers

```csharp
// Notification/Verendar.Notification/Bootstrapping/ApplicationServiceExtensions.cs
public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
{
    builder.AddServiceDefaults();
    builder.AddCommonService();
    builder.AddPostgresDatabase<NotificationDbContext>(Const.NotificationDatabase);

    // Đăng ký MassTransit (Aspire helper tự cấu hình RabbitMQ connection)
    builder.AddRabbitMqEventBus(configurator =>
    {
        configurator.AddConsumer<UserRegisteredConsumer>();
        configurator.AddConsumer<BrandLogoMediaSupersededConsumer>();
        // thêm consumer mới ở đây
    });

    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    return builder;
}
```

`AddRabbitMqEventBus` là extension trong `Verendar.Common` — wrap `AddMassTransit` với cấu hình chuẩn cho RabbitMQ.

---

## Tổng kết: Chọn HTTP hay Message Queue?

| Tiêu chí | HTTP (IHttpClientFactory) | MassTransit (RabbitMQ) |
|---|---|---|
| Cần response ngay | ✓ | ✗ |
| Có thể fail gracefully | Cần thiết kế result type | Retry tự động |
| Một publisher, nhiều consumer | ✗ | ✓ |
| Audit/traceability | Log HTTP request | MessageId correlation |
| Coupling | Tight (biết URL target) | Loose (chỉ biết contract) |

**Rule of thumb:**
- Internal data fetch → HTTP (`/api/internal/...`)
- "Something happened, others should react" → MassTransit publish
- Payment (Garage → Payment) → HTTP initiate, MassTransit consume result
