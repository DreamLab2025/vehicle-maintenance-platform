using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Garage.Application.ExternalServices;
using Verendar.Garage.Infrastructure.Configuration;

namespace Verendar.Garage.Infrastructure.ExternalServices.Geocoding;

public class NominatimGeocodingService(
    IOptions<GeocodingSettings> options,
    IHttpClientFactory httpClientFactory,
    ILogger<NominatimGeocodingService> logger) : IGeocodingService
{
    private readonly GeocodingSettings _settings = options.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<NominatimGeocodingService> _logger = logger;

    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_settings.BaseUrl);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", _settings.UserAgent);

            var query = Uri.EscapeDataString(address);
            var results = await client.GetFromJsonAsync<NominatimResult[]>(
                $"/search?q={query}&format=jsonv2&limit=1", ct);

            if (results is null || results.Length == 0)
            {
                _logger.LogWarning("Nominatim geocoding: no results for address '{Address}'", address);
                return null;
            }

            if (double.TryParse(results[0].Lat, System.Globalization.CultureInfo.InvariantCulture, out var lat)
                && double.TryParse(results[0].Lon, System.Globalization.CultureInfo.InvariantCulture, out var lon))
            {
                return (lat, lon);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nominatim geocoding error for address '{Address}'", address);
            return null;
        }
    }

    private sealed record NominatimResult(
        [property: JsonPropertyName("lat")] string Lat,
        [property: JsonPropertyName("lon")] string Lon);
}
