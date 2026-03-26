namespace Verendar.Garage.Application.Clients;

public interface ILocationClient
{
    Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default);
}
