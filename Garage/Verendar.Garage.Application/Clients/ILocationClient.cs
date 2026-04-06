using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Clients;

public interface ILocationClient
{
    Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default);
    Task<MapLinksDto?> GetMapLinksAsync(double lat, double lng, CancellationToken ct = default);
    Task<(bool IsValid, string? ProvinceName, string? WardName)> ValidateLocationAsync(
        string provinceCode, string wardCode, CancellationToken ct = default);
}
