using Verendar.Location.Application.ExternalServices;

namespace Verendar.Location.Infrastructure.ExternalServices.Geocoding;

public sealed class NullGeocodingService : IGeocodingService
{
    public Task<(double Latitude, double Longitude)?> GeocodeAsync(string address, CancellationToken ct = default) =>
        Task.FromResult<(double Latitude, double Longitude)?>(null);
}
