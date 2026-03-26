using Amazon.GeoPlaces;
using Amazon.GeoPlaces.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Location.Application.ExternalServices;
using Verendar.Location.Infrastructure.Configuration;

namespace Verendar.Location.Infrastructure.ExternalServices.Geocoding;

public class AwsGeocodingService(
    IAmazonGeoPlaces geoPlacesClient,
    IOptions<AwsGeocodingSettings> options,
    ILogger<AwsGeocodingService> logger) : IGeocodingService
{
    private readonly IAmazonGeoPlaces _geoPlacesClient = geoPlacesClient;
    private readonly AwsGeocodingSettings _settings = options.Value;
    private readonly ILogger<AwsGeocodingService> _logger = logger;

    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default)
    {
        try
        {
            var request = new GeocodeRequest
            {
                QueryText = address,
                Filter = new GeocodeFilter { IncludeCountries = ["VNM"] },
                Language = "vi",
                MaxResults = 1
            };

            var response = await _geoPlacesClient.GeocodeAsync(request, ct);

            if (response.ResultItems is null || response.ResultItems.Count == 0)
            {
                _logger.LogWarning("AWS GeoPlaces: no results for address '{Address}'", address);
                return null;
            }

            var item = response.ResultItems[0];

            if (item.MatchScores?.Overall < _settings.MinMatchScore)
            {
                _logger.LogWarning(
                    "AWS GeoPlaces: low confidence {Score:F2} (threshold {Threshold:F2}) for address '{Address}'",
                    item.MatchScores.Overall, _settings.MinMatchScore, address);
                return null;
            }

            if (item.Position is null || item.Position.Count < 2)
                return null;

            // AWS returns [lng, lat] — swap to (lat, lng)
            return (item.Position[1], item.Position[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AWS GeoPlaces geocoding error for address '{Address}'", address);
            return null;
        }
    }
}
