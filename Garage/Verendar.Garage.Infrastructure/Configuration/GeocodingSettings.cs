namespace Verendar.Garage.Infrastructure.Configuration;

public class GeocodingSettings
{
    public const string SectionName = "Geocoding";

    public string BaseUrl { get; set; } = "https://nominatim.openstreetmap.org";

    public string? ApiKey { get; set; }

    public string UserAgent { get; set; } = "Verendar/1.0";
}
