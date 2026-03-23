using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Verendar.Garage.Infrastructure.Configuration;
using Verendar.Garage.Infrastructure.ExternalServices.Geocoding;

namespace Verendar.Garage.Tests.ExternalServices;

public class NominatimGeocodingServiceTests
{
    [Fact]
    public async Task GeocodeAsync_WhenNominatimReturnsHit_ReturnsCoordinates()
    {
        var handler = new TestHttpMessageHandler
        {
            SendImpl = req =>
            {
                req.RequestUri!.PathAndQuery.Should().Contain("/search");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """[{"lat":"10.762622","lon":"106.660172"}]""",
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler) { BaseAddress = new Uri("https://nominatim.openstreetmap.org") });

        var options = Options.Create(new GeocodingSettings
        {
            BaseUrl = "https://nominatim.openstreetmap.org",
            UserAgent = "Verendar.Tests/1.0"
        });
        var sut = new NominatimGeocodingService(
            options,
            factory.Object,
            NullLogger<NominatimGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("District 1, HCMC");

        result.Should().NotBeNull();
        result!.Value.Latitude.Should().Be(10.762622);
        result.Value.Longitude.Should().Be(106.660172);
    }

    [Fact]
    public async Task GeocodeAsync_WhenNominatimReturnsEmptyArray_ReturnsNull()
    {
        var handler = new TestHttpMessageHandler
        {
            SendImpl = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            }
        };

        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler) { BaseAddress = new Uri("https://nominatim.openstreetmap.org") });

        var options = Options.Create(new GeocodingSettings
        {
            BaseUrl = "https://nominatim.openstreetmap.org",
            UserAgent = "Verendar.Tests/1.0"
        });
        var sut = new NominatimGeocodingService(
            options,
            factory.Object,
            NullLogger<NominatimGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("___unlikely___");

        result.Should().BeNull();
    }
}
