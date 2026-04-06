namespace Verendar.Location.Application.Dtos;

public record GeocodeResponse(double? Latitude, double? Longitude);

public record ReverseGeocodeResponse(string? Address);

public record MapLinksResponse(
    string GoogleMaps,
    string AppleMaps,
    string Waze,
    string OpenStreetMap)
{
    public static MapLinksResponse From(double lat, double lng)
    {
        var latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lngStr = lng.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return new MapLinksResponse(
            GoogleMaps: $"https://www.google.com/maps?q={latStr},{lngStr}",
            AppleMaps: $"https://maps.apple.com/?ll={latStr},{lngStr}",
            Waze: $"https://waze.com/ul?ll={latStr},{lngStr}&navigate=yes",
            OpenStreetMap: $"https://www.openstreetmap.org/?mlat={latStr}&mlon={lngStr}&zoom=17"
        );
    }
}
