using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Location.Application.ExternalServices;
using Verendar.Location.Infrastructure.Configuration;

namespace Verendar.Location.Infrastructure.ExternalServices.Geocoding;

public class GoogleMapsGeocodingService(
    IOptions<GeocodingSettings> options,
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleMapsGeocodingService> logger) : IGeocodingService
{
    private readonly GeocodingSettings _settings = options.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<GoogleMapsGeocodingService> _logger = logger;

    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogWarning("Google Maps geocoding: ApiKey is not configured");
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var query = Uri.EscapeDataString(address);
            var url =
                $"https://maps.googleapis.com/maps/api/geocode/json?address={query}&key={Uri.EscapeDataString(_settings.ApiKey)}";

            var result = await client.GetFromJsonAsync<GoogleGeocodeResponse>(url, ct);

            if (result is null || result.Status != "OK" || result.Results is null || result.Results.Length == 0)
            {
                _logger.LogWarning(
                    "Google Maps geocoding: no results for address '{Address}'. Status={Status}",
                    address,
                    result?.Status);
                return null;
            }

            var loc = result.Results[0].Geometry?.Location;
            if (loc is null)
                return null;

            return (loc.Lat, loc.Lng);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Maps geocoding error for address '{Address}'", address);
            return null;
        }
    }

    private sealed record GoogleGeocodeResponse(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("results")] GoogleGeocodeResult[]? Results);

    private sealed record GoogleGeocodeResult(
        [property: JsonPropertyName("geometry")] GoogleGeometry? Geometry);

    private sealed record GoogleGeometry(
        [property: JsonPropertyName("location")] GoogleLocation? Location);

    private sealed record GoogleLocation(
        [property: JsonPropertyName("lat")] double Lat,
        [property: JsonPropertyName("lng")] double Lng);
}
