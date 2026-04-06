using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class ReviewMappings
{
    public static GarageReviewResponse ToResponse(this GarageReview review) => new()
    {
        Id = review.Id,
        BookingId = review.BookingId,
        GarageBranchId = review.GarageBranchId,
        UserId = review.UserId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };
}
