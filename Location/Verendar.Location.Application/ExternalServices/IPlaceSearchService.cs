namespace Verendar.Location.Application.ExternalServices;

public interface IPlaceSearchService
{
    Task<List<PlaceSuggestion>> SearchAsync(string query, string? provinceName, string? wardName, string? sessionToken, CancellationToken ct = default);
    Task<PlaceDetail?> GetDetailsAsync(string placeId, string? sessionToken, CancellationToken ct = default);
}

public record PlaceSuggestion(string PlaceId, string Description, string MainText, string SecondaryText);
public record PlaceDetail(string PlaceId, string FormattedAddress, double Latitude, double Longitude);
