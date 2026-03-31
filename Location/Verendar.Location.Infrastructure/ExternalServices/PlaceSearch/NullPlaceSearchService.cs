using Verendar.Location.Application.ExternalServices;

namespace Verendar.Location.Infrastructure.ExternalServices.PlaceSearch;

public class NullPlaceSearchService : IPlaceSearchService
{
    public Task<List<PlaceSuggestion>> SearchAsync(
        string query,
        string? provinceName,
        string? wardName,
        string? sessionToken,
        CancellationToken ct = default)
        => Task.FromResult<List<PlaceSuggestion>>([]);

    public Task<PlaceDetail?> GetDetailsAsync(
        string placeId,
        string? sessionToken,
        CancellationToken ct = default)
        => Task.FromResult<PlaceDetail?>(null);
}
