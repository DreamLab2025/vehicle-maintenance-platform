using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Clients;

public interface ILocationClient
{
    Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default);
    Task<MapLinksDto?> GetMapLinksAsync(double lat, double lng, CancellationToken ct = default);
}
