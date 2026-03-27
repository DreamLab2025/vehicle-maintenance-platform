using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Xunit;

namespace Verendar.AppHost.Tests.Fixtures;

public sealed class AppHostFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public HttpClient LocationClient { get; private set; } = null!;
    public HttpClient GarageClient { get; private set; } = null!;
    public HttpClient ApiGatewayClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Verendar_AppHost>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        await _app.ResourceNotifications.WaitForResourceHealthyAsync("location-service");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("garage-service");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("api-gateway");

        LocationClient = _app.CreateHttpClient("location-service");
        GarageClient = _app.CreateHttpClient("garage-service");
        ApiGatewayClient = _app.CreateHttpClient("api-gateway");
    }

    public async Task DisposeAsync()
    {
        LocationClient.Dispose();
        GarageClient.Dispose();
        ApiGatewayClient.Dispose();

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
