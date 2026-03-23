using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Verendar.Garage.Infrastructure.Configuration;
using Verendar.Garage.Infrastructure.ExternalServices.Geocoding;

namespace Verendar.Garage.Tests.ExternalServices;

public class GoogleMapsGeocodingServiceTests
{
    [Fact]
    public async Task GeocodeAsync_WhenApiKeyMissing_ReturnsNullWithoutCallingHttp()
    {
        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);

        var options = Options.Create(new GeocodingSettings { ApiKey = null });
        var sut = new GoogleMapsGeocodingService(
            options,
            factory.Object,
            NullLogger<GoogleMapsGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("Any address");

        result.Should().BeNull();
        factory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GeocodeAsync_WhenGoogleReturnsOk_ReturnsCoordinates()
    {
        var handler = new TestHttpMessageHandler
        {
            SendImpl = req =>
            {
                req.RequestUri!.Host.Should().Be("maps.googleapis.com");
                req.RequestUri!.AbsolutePath.Should().Be("/maps/api/geocode/json");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"status":"OK","results":[{"geometry":{"location":{"lat":10.5,"lng":106.6}}}]}""",
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler));

        var options = Options.Create(new GeocodingSettings { ApiKey = "test-key" });
        var sut = new GoogleMapsGeocodingService(
            options,
            factory.Object,
            NullLogger<GoogleMapsGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("Ho Chi Minh City");

        result.Should().NotBeNull();
        result!.Value.Latitude.Should().Be(10.5);
        result.Value.Longitude.Should().Be(106.6);
    }
}
