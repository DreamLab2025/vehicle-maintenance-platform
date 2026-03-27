using System.Net;
using FluentAssertions;
using Verendar.AppHost.Tests.Fixtures;
using Verendar.AppHost.Tests.Support;
using Xunit;

namespace Verendar.AppHost.Tests.Flows;

[Collection(AppHostCollection.Name)]
public class LocationGarageFlowTests(AppHostFixture fixture)
{
    [Fact]
    public async Task ServiceFlow_WhenCallingLocationThenGarageRoutes_EndpointsShouldRespondSuccessfully()
    {
        var locationResponse = await fixture.LocationClient.GetAsync("/api/v1/locations/provinces");
        locationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var locationPayload = await locationResponse.Content.ReadAsStringAsync();
        var firstProvinceCode = ApiResponseJsonReader.ReadFirstArrayItemStringProperty(locationPayload, "code");
        firstProvinceCode.Should().NotBeNullOrWhiteSpace();

        var wardsResponse = await fixture.LocationClient.GetAsync($"/api/v1/locations/provinces/{firstProvinceCode}/wards");
        wardsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var serviceCategoryResponse = await fixture.GarageClient.GetAsync("/api/v1/service-categories");
        serviceCategoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
