using Verendar.Location.Application.Dtos;

namespace Verendar.Location.Tests.Dtos;

public class MapLinksResponseTests
{
    [Fact]
    public void From_ProducesCorrectGoogleMapsUrl()
    {
        var result = MapLinksResponse.From(10.762622, 106.660172);

        result.GoogleMaps.Should().Be("https://www.google.com/maps?q=10.762622,106.660172");
    }

    [Fact]
    public void From_ProducesCorrectAppleMapsUrl()
    {
        var result = MapLinksResponse.From(10.762622, 106.660172);

        result.AppleMaps.Should().Be("https://maps.apple.com/?ll=10.762622,106.660172");
    }

    [Fact]
    public void From_ProducesCorrectWazeUrl()
    {
        var result = MapLinksResponse.From(10.762622, 106.660172);

        result.Waze.Should().Be("https://waze.com/ul?ll=10.762622,106.660172&navigate=yes");
    }

    [Fact]
    public void From_ProducesCorrectOpenStreetMapUrl()
    {
        var result = MapLinksResponse.From(10.762622, 106.660172);

        result.OpenStreetMap.Should().Be("https://www.openstreetmap.org/?mlat=10.762622&mlon=106.660172&zoom=17");
    }

    [Fact]
    public void From_UsesInvariantCulture_DecimalPointNotComma()
    {
        // Coordinates with fractional parts must use '.' not ',' regardless of system locale
        var result = MapLinksResponse.From(10.5, 106.7);

        result.GoogleMaps.Should().Contain("10.5").And.Contain("106.7");
        result.GoogleMaps.Should().NotContain("10,5").And.NotContain("106,7");
    }

    [Fact]
    public void From_WithNegativeCoordinates_ProducesValidUrls()
    {
        // Southern hemisphere / western longitude (e.g. São Paulo)
        var result = MapLinksResponse.From(-23.5505, -46.6333);

        result.GoogleMaps.Should().Be("https://www.google.com/maps?q=-23.5505,-46.6333");
        result.AppleMaps.Should().Be("https://maps.apple.com/?ll=-23.5505,-46.6333");
        result.Waze.Should().Be("https://waze.com/ul?ll=-23.5505,-46.6333&navigate=yes");
        result.OpenStreetMap.Should().Be("https://www.openstreetmap.org/?mlat=-23.5505&mlon=-46.6333&zoom=17");
    }

    [Fact]
    public void From_WithZeroCoordinates_ProducesValidUrls()
    {
        var result = MapLinksResponse.From(0, 0);

        result.GoogleMaps.Should().Be("https://www.google.com/maps?q=0,0");
        result.AppleMaps.Should().Be("https://maps.apple.com/?ll=0,0");
    }

    [Fact]
    public void From_AllFourLinksArePopulated()
    {
        var result = MapLinksResponse.From(21.0285, 105.8542);

        result.GoogleMaps.Should().NotBeNullOrEmpty();
        result.AppleMaps.Should().NotBeNullOrEmpty();
        result.Waze.Should().NotBeNullOrEmpty();
        result.OpenStreetMap.Should().NotBeNullOrEmpty();
    }
}
