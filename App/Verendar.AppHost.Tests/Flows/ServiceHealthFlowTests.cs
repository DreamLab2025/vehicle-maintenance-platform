using System.Net;
using FluentAssertions;
using Verendar.AppHost.Tests.Fixtures;
using Xunit;

namespace Verendar.AppHost.Tests.Flows;

[Collection(AppHostCollection.Name)]
public class ServiceHealthFlowTests(AppHostFixture fixture)
{
    [Fact]
    public async Task ServiceHealth_WhenRunningUnderAspire_ShouldExposeHealthyEndpoints()
    {
        var locationHealth = await fixture.LocationClient.GetAsync("/health");
        var garageHealth = await fixture.GarageClient.GetAsync("/health");

        locationHealth.StatusCode.Should().Be(HttpStatusCode.OK);
        garageHealth.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
