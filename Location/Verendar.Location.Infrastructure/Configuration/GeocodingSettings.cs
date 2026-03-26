namespace Verendar.Location.Infrastructure.Configuration;

public class GeocodingSettings
{
    public const string SectionName = "Geocoding";

    public string? ApiKey { get; set; }
}
