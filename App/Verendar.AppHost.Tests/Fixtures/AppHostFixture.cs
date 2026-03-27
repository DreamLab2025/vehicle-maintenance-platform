using System.Net;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Verendar.AppHost.Tests.Support;
using Xunit;

namespace Verendar.AppHost.Tests.Fixtures;

public sealed class AppHostFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public HttpClient IdentityClient { get; private set; } = null!;
    public HttpClient VehicleClient { get; private set; } = null!;
    public HttpClient AiClient { get; private set; } = null!;
    public HttpClient NotificationClient { get; private set; } = null!;
    public HttpClient LocationClient { get; private set; } = null!;
    public HttpClient GarageClient { get; private set; } = null!;
    public HttpClient ApiGatewayClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("VERENDAR_TEST_ISOLATED", "1");

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Verendar_AppHost>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        await _app.ResourceNotifications.WaitForResourceHealthyAsync("identity-service");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("vehicle-service");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("ai-service");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("notification-service");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("location-service");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("garage-service");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("api-gateway");

        IdentityClient = _app.CreateHttpClient("identity-service");
        VehicleClient = _app.CreateHttpClient("vehicle-service");
        AiClient = _app.CreateHttpClient("ai-service");
        NotificationClient = _app.CreateHttpClient("notification-service");
        LocationClient = _app.CreateHttpClient("location-service");
        GarageClient = _app.CreateHttpClient("garage-service");
        ApiGatewayClient = _app.CreateHttpClient("api-gateway");

        ConfigureHttpClient(IdentityClient);
        ConfigureHttpClient(VehicleClient);
        ConfigureHttpClient(AiClient);
        ConfigureHttpClient(NotificationClient);
        ConfigureHttpClient(LocationClient);
        ConfigureHttpClient(GarageClient);
        ConfigureHttpClient(ApiGatewayClient);

        await TestDataInitializer.InitializeAsync(this);
    }

    public async Task DisposeAsync()
    {
        IdentityClient.Dispose();
        VehicleClient.Dispose();
        AiClient.Dispose();
        NotificationClient.Dispose();
        LocationClient.Dispose();
        GarageClient.Dispose();
        ApiGatewayClient.Dispose();

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }

        Environment.SetEnvironmentVariable("VERENDAR_TEST_ISOLATED", null);
    }

    private static void ConfigureHttpClient(HttpClient client)
    {
        client.DefaultRequestVersion = HttpVersion.Version11;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
    }
}
