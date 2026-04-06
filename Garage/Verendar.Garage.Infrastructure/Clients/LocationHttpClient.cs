using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Infrastructure.Clients;

public class LocationHttpClient(HttpClient httpClient, ILogger<LocationHttpClient> logger) : ILocationClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<LocationHttpClient> _logger = logger;

    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default)
    {
        try
        {
            var encoded = Uri.EscapeDataString(address);
            var response = await _httpClient.GetFromJsonAsync<GeocodeResponse>(
                $"/api/internal/locations/geocode?address={encoded}", ct);

            if (response?.Latitude is null || response.Longitude is null)
                return null;

            return (response.Latitude.Value, response.Longitude.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Location service geocoding error for address '{Address}'", address);
            return null;
        }
    }

    public async Task<MapLinksDto?> GetMapLinksAsync(double lat, double lng, CancellationToken ct = default)
    {
        try
        {
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(CultureInfo.InvariantCulture);
            var response = await _httpClient.GetFromJsonAsync<MapLinksResponse>(
                $"/api/internal/locations/map-links?lat={latStr}&lng={lngStr}", ct);

            if (response is null)
                return null;

            return new MapLinksDto
            {
                GoogleMaps = response.GoogleMaps,
                AppleMaps = response.AppleMaps,
                Waze = response.Waze,
                OpenStreetMap = response.OpenStreetMap
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Location service map-links error for ({Lat},{Lng})", lat, lng);
            return null;
        }
    }

    public async Task<(bool IsValid, string? ProvinceName, string? WardName)> ValidateLocationAsync(
        string provinceCode, string wardCode, CancellationToken ct = default)
    {
        try
        {
            var encodedProvince = Uri.EscapeDataString(provinceCode);
            var encodedWard = Uri.EscapeDataString(wardCode);
            var response = await _httpClient.GetAsync(
                $"/api/internal/locations/validate?provinceCode={encodedProvince}&wardCode={encodedWard}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Location validation returned {Status} for province '{Province}', ward '{Ward}'",
                    response.StatusCode, provinceCode, wardCode);
                return (false, null, null);
            }

            var result = await response.Content.ReadFromJsonAsync<LocationValidationResult>(cancellationToken: ct);
            return result is { IsValid: true }
                ? (true, result.ProvinceName, result.WardName)
                : (false, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Location validation error for province '{Province}', ward '{Ward}'",
                provinceCode, wardCode);
            return (false, null, null);
        }
    }

    private sealed record GeocodeResponse(
        [property: JsonPropertyName("latitude")] double? Latitude,
        [property: JsonPropertyName("longitude")] double? Longitude);

    private sealed record MapLinksResponse(
        [property: JsonPropertyName("googleMaps")] string GoogleMaps,
        [property: JsonPropertyName("appleMaps")] string AppleMaps,
        [property: JsonPropertyName("waze")] string Waze,
        [property: JsonPropertyName("openStreetMap")] string OpenStreetMap);

    private sealed record LocationValidationResult(
        [property: JsonPropertyName("isValid")] bool IsValid,
        [property: JsonPropertyName("provinceName")] string? ProvinceName,
        [property: JsonPropertyName("wardName")] string? WardName);
}
