namespace Verendar.Garage.Application.Dtos;

public record CreateReviewRequest
{
    public int Rating { get; init; }
    public string? Comment { get; init; }
}

public record GarageReviewResponse
{
    public Guid Id { get; init; }
    public Guid BookingId { get; init; }
    public Guid GarageBranchId { get; init; }
    public Guid UserId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
}
