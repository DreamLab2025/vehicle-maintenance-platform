# TDD — .NET / C# Patterns (Verendar)

## Unit Test — xUnit + FluentAssertions + NSubstitute

```csharp
// Verendar.Garage.Tests/Services/BookingServiceTests.cs
public class BookingServiceTests
{
    private readonly IBookingRepository _bookingRepo = Substitute.For<IBookingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _unitOfWork.Bookings.Returns(_bookingRepo);
        _sut = new BookingService(_unitOfWork, _publishEndpoint, NullLogger<BookingService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccessAndPublishesEvent()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            VehicleId      = Guid.NewGuid(),
            GarageBranchId = Guid.NewGuid(),
            ScheduledAt    = DateTime.UtcNow.AddDays(1),
        };

        // Act
        var result = await _sut.CreateAsync(request, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        await _publishEndpoint.Received(1).Publish(
            Arg.Any<BookingCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_PastScheduledAt_ReturnsFailure()
    {
        var request = new CreateBookingRequest { ScheduledAt = DateTime.UtcNow.AddDays(-1) };

        var result = await _sut.CreateAsync(request, Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("scheduled");
    }
}
```

## Integration Test — WebApplicationFactory

```csharp
// Verendar.Garage.Tests/Integration/BookingApiTests.cs
public class BookingApiTests(GarageWebFactory factory) : IClassFixture<GarageWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task POST_Bookings_ValidRequest_Returns201()
    {
        // Arrange
        var token = factory.GenerateUserToken(userId: Guid.NewGuid());
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateBookingRequest
        {
            VehicleId      = Guid.NewGuid(),
            GarageBranchId = Guid.NewGuid(),
            ScheduledAt    = DateTime.UtcNow.AddDays(1),
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/bookings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<BookingResponse>>();
        body!.Data.Should().NotBeNull();
        body.Data!.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task POST_Bookings_Unauthenticated_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/bookings", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

## Test Factory with Testcontainers

```csharp
// Verendar.Garage.Tests/GarageWebFactory.cs
public class GarageWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("garage_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace real DB with Testcontainers instance
            services.RemoveAll<DbContextOptions<GarageDbContext>>();
            services.AddDbContext<GarageDbContext>(opts =>
                opts.UseNpgsql(_postgres.GetConnectionString()));

            // Replace MassTransit with test harness (no real RabbitMQ)
            services.AddMassTransitTestHarness();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<GarageDbContext>()
            .Database.MigrateAsync();
    }

    public new async Task DisposeAsync() => await _postgres.StopAsync();

    public string GenerateUserToken(Guid userId, string role = "User") => /* JWT helper */;
}
```

## Parameterized Tests

```csharp
[Theory]
[InlineData("", false)]           // empty
[InlineData("abc", false)]        // too short
[InlineData("valid@email.com", true)]
[InlineData("VALID@EMAIL.COM", true)]
public void IsValidEmail_VariousInputs_ReturnsExpected(string email, bool expected)
{
    var result = EmailValidator.IsValid(email);
    result.Should().Be(expected);
}
```

## Running Tests in Verendar

```bash
task test:all                                                     # all tests
task test PROJECT=Garage/Verendar.Garage.Tests                    # single project
dotnet test --filter "FullyQualifiedName~BookingServiceTests"     # single class
```
