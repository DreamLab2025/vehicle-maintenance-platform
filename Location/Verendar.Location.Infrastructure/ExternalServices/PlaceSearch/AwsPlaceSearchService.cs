using Amazon.GeoPlaces;
using Amazon.GeoPlaces.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using Verendar.Location.Application.ExternalServices;

namespace Verendar.Location.Infrastructure.ExternalServices.PlaceSearch;

public class AwsPlaceSearchService(
    IAmazonGeoPlaces geoPlacesClient,
    ILogger<AwsPlaceSearchService> logger) : IPlaceSearchService
{
    private readonly IAmazonGeoPlaces _geoPlacesClient = geoPlacesClient;
    private readonly ILogger<AwsPlaceSearchService> _logger = logger;

    public async Task<List<PlaceSuggestion>> SearchAsync(
        string query,
        string? provinceName,
        string? wardName,
        string? sessionToken,
        CancellationToken ct = default)
    {
        try
        {
            var input = query;
            if (wardName is not null) input = $"{input}, {wardName}";
            if (provinceName is not null) input = $"{input}, {provinceName}";

            var request = new SuggestRequest
            {
                QueryText = input,
                Filter = new SuggestFilter { IncludeCountries = ["VNM"] },
                Language = "vi",
                MaxResults = 5,
                IntendedUse = SuggestIntendedUse.SingleUse
            };

            var response = await _geoPlacesClient.SuggestAsync(request, ct);

            if (response.ResultItems is null || response.ResultItems.Count == 0)
                return [];

            var results = new List<PlaceSuggestion>();

            foreach (var item in response.ResultItems)
            {
                // Only include place results (not query suggestions)
                if (item.SuggestResultItemType != SuggestResultItemType.Place)
                    continue;

                var placeId = item.Place?.PlaceId;
                if (string.IsNullOrEmpty(placeId))
                    continue;

                var title = item.Title ?? string.Empty;
                var (mainText, secondaryText) = SplitDescription(title);

                results.Add(new PlaceSuggestion(placeId, title, mainText, secondaryText));
            }

            return results;
        }
        catch (AmazonServiceException ex) when (
            ex.Message.Contains("IAM security credentials", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Unable to get IAM", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("AWS GeoPlaces: unable to get IAM credentials for place search query '{Query}'", query);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AWS GeoPlaces place search error for query '{Query}'", query);
            return [];
        }
    }

    public async Task<PlaceDetail?> GetDetailsAsync(
        string placeId,
        string? sessionToken,
        CancellationToken ct = default)
    {
        try
        {
            var request = new GetPlaceRequest
            {
                PlaceId = placeId,
                Language = "vi",
                AdditionalFeatures = [GetPlaceAdditionalFeature.Contact]
            };

            var response = await _geoPlacesClient.GetPlaceAsync(request, ct);

            if (response.Position is null || response.Position.Count < 2)
            {
                _logger.LogWarning("AWS GeoPlaces: no position data for placeId '{PlaceId}'", placeId);
                return null;
            }

            // AWS returns [lng, lat] — swap to (lat, lng)
            var latitude = response.Position[1];
            var longitude = response.Position[0];

            var address = response.Address?.Label ?? string.Empty;

            return new PlaceDetail(placeId, address, latitude, longitude);
        }
        catch (AmazonServiceException ex) when (
            ex.Message.Contains("IAM security credentials", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Unable to get IAM", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("AWS GeoPlaces: unable to get IAM credentials for placeId '{PlaceId}'", placeId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AWS GeoPlaces place details error for placeId '{PlaceId}'", placeId);
            return null;
        }
    }

    /// <summary>
    /// Splits "Street Name, District, City, Country" into (mainText, secondaryText).
    /// mainText = first segment, secondaryText = remainder.
    /// </summary>
    private static (string MainText, string SecondaryText) SplitDescription(string description)
    {
        var commaIndex = description.IndexOf(',');
        if (commaIndex < 0)
            return (description, string.Empty);

        return (
            description[..commaIndex].Trim(),
            description[(commaIndex + 1)..].Trim()
        );
    }
}
