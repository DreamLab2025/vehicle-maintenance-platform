namespace Verendar.Location.Application.ExternalServices;

public interface IGeocodingService
{
    Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default);
    Task<string?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken ct = default);
}
