namespace Verendar.Location.Infrastructure.Configuration;

public class AwsGeocodingSettings
{
    public const string SectionName = "Geocoding:AWS";

    public double MinMatchScore { get; set; } = 0.7;
}

public class GoogleGeocodingSettings
{
    public const string SectionName = "Geocoding:Google";

    public string? ApiKey { get; set; }
}
