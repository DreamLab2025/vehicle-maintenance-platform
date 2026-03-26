using Amazon.GeoPlaces;
using Amazon.GeoPlaces.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Verendar.Location.Infrastructure.Configuration;
using Verendar.Location.Infrastructure.ExternalServices.Geocoding;

namespace Verendar.Location.Tests.ExternalServices;

public class AwsGeocodingServiceTests
{
    private static AwsGeocodingSettings DefaultSettings() => new() { MinMatchScore = 0.7 };

    private static GeocodeResultItem MakeItem(double lng, double lat, double score) =>
        new()
        {
            Position = [lng, lat],
            MatchScores = new MatchScoreDetails { Overall = score }
        };

    [Fact]
    public async Task GeocodeAsync_WhenValidResult_ReturnsSwappedLatLng()
    {
        // AWS trả [lng, lat] → service phải swap thành (lat, lng)
        var mockClient = new Mock<IAmazonGeoPlaces>(MockBehavior.Strict);
        mockClient
            .Setup(c => c.GeocodeAsync(It.IsAny<GeocodeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodeResponse
            {
                ResultItems = [MakeItem(lng: 106.660172, lat: 10.762622, score: 0.95)]
            });

        var sut = new AwsGeocodingService(
            mockClient.Object,
            Options.Create(DefaultSettings()),
            NullLogger<AwsGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("123 Nguyễn Trãi, Quận 5, TP.HCM");

        result.Should().NotBeNull();
        result!.Value.Latitude.Should().Be(10.762622);
        result.Value.Longitude.Should().Be(106.660172);
    }

    [Fact]
    public async Task GeocodeAsync_WhenResultItemsEmpty_ReturnsNull()
    {
        var mockClient = new Mock<IAmazonGeoPlaces>(MockBehavior.Strict);
        mockClient
            .Setup(c => c.GeocodeAsync(It.IsAny<GeocodeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodeResponse { ResultItems = [] });

        var sut = new AwsGeocodingService(
            mockClient.Object,
            Options.Create(DefaultSettings()),
            NullLogger<AwsGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("địa chỉ không tồn tại");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WhenScoreBelowThreshold_ReturnsNull()
    {
        var mockClient = new Mock<IAmazonGeoPlaces>(MockBehavior.Strict);
        mockClient
            .Setup(c => c.GeocodeAsync(It.IsAny<GeocodeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodeResponse
            {
                ResultItems = [MakeItem(lng: 106.6, lat: 10.7, score: 0.5)]  // < 0.7 threshold
            });

        var sut = new AwsGeocodingService(
            mockClient.Object,
            Options.Create(DefaultSettings()),
            NullLogger<AwsGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("địa chỉ mơ hồ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WhenSdkThrows_ReturnsNull()
    {
        var mockClient = new Mock<IAmazonGeoPlaces>(MockBehavior.Strict);
        mockClient
            .Setup(c => c.GeocodeAsync(It.IsAny<GeocodeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonGeoPlacesException("service unavailable"));

        var sut = new AwsGeocodingService(
            mockClient.Object,
            Options.Create(DefaultSettings()),
            NullLogger<AwsGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("bất kỳ địa chỉ nào");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WhenValidationExceptionThrown_ReturnsNull()
    {
        // ValidationException is thrown by AWS when the operation is not supported in the configured region
        // (e.g. "Operation Geocode is not supported in ap-southeast-1")
        var mockClient = new Mock<IAmazonGeoPlaces>(MockBehavior.Strict);
        mockClient
            .Setup(c => c.GeocodeAsync(It.IsAny<GeocodeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Amazon.GeoPlaces.Model.ValidationException("Operation Geocode is not supported in ap-southeast-1"));

        var sut = new AwsGeocodingService(
            mockClient.Object,
            Options.Create(DefaultSettings()),
            NullLogger<AwsGeocodingService>.Instance);

        var result = await sut.GeocodeAsync("123 Lê Lợi, Q1");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_RequestIncludesVietnamFilterAndVietnameseLanguage()
    {
        GeocodeRequest? capturedRequest = null;

        var mockClient = new Mock<IAmazonGeoPlaces>(MockBehavior.Strict);
        mockClient
            .Setup(c => c.GeocodeAsync(It.IsAny<GeocodeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GeocodeRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new GeocodeResponse { ResultItems = [] });

        var sut = new AwsGeocodingService(
            mockClient.Object,
            Options.Create(DefaultSettings()),
            NullLogger<AwsGeocodingService>.Instance);

        await sut.GeocodeAsync("123 Lê Lợi, Q1");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.QueryText.Should().Be("123 Lê Lợi, Q1");
        capturedRequest.Filter.IncludeCountries.Should().Contain("VNM");
        capturedRequest.Language.Should().Be("vi");
        capturedRequest.MaxResults.Should().Be(1);
    }
}
