namespace Verendar.Location.Application.Dtos;

public class PlaceSuggestionResponse
{
    public string PlaceId { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string MainText { get; set; } = null!;
    public string SecondaryText { get; set; } = null!;
}

public class PlaceDetailResponse
{
    public string PlaceId { get; set; } = null!;
    public string FormattedAddress { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
