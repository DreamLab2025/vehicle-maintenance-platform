using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Location.Application.ExternalServices;
using Verendar.Location.Infrastructure.Configuration;

namespace Verendar.Location.Infrastructure.ExternalServices.Geocoding;

public class GoogleMapsGeocodingService(
    IOptions<GoogleGeocodingSettings> options,
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleMapsGeocodingService> logger) : IGeocodingService
{
    private readonly GoogleGeocodingSettings _settings = options.Value;
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

    public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogWarning("Google Maps reverse geocoding: ApiKey is not configured");
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var latStr = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var lngStr = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var url =
                $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latStr},{lngStr}&language=vi&key={Uri.EscapeDataString(_settings.ApiKey)}";

            var result = await client.GetFromJsonAsync<GoogleGeocodeResponse>(url, ct);

            if (result is null || result.Status != "OK" || result.Results is null || result.Results.Length == 0)
            {
                _logger.LogWarning(
                    "Google Maps reverse geocoding: no results for ({Lat},{Lng}). Status={Status}",
                    latitude, longitude, result?.Status);
                return null;
            }

            return result.Results[0].FormattedAddress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Maps reverse geocoding error for ({Lat},{Lng})", latitude, longitude);
            return null;
        }
    }

    private sealed record GoogleGeocodeResponse(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("results")] GoogleGeocodeResult[]? Results);

    private sealed record GoogleGeocodeResult(
        [property: JsonPropertyName("formatted_address")] string? FormattedAddress,
        [property: JsonPropertyName("geometry")] GoogleGeometry? Geometry);

    private sealed record GoogleGeometry(
        [property: JsonPropertyName("location")] GoogleLocation? Location);

    private sealed record GoogleLocation(
        [property: JsonPropertyName("lat")] double Lat,
        [property: JsonPropertyName("lng")] double Lng);
}
