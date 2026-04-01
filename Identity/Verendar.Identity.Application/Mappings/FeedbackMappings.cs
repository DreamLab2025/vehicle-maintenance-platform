using Verendar.Identity.Domain.Entities;

namespace Verendar.Identity.Application.Mappings;

public static class FeedbackMappings
{
    public static Feedback ToEntity(this CreateFeedbackRequest request, Guid userId) => new()
    {
        UserId = userId,
        Category = request.Category,
        Subject = request.Subject,
        Content = request.Content,
        Rating = request.Rating,
        ContactEmail = request.ContactEmail
    };

    public static FeedbackResponse ToResponse(this Feedback feedback) => new()
    {
        Id = feedback.Id,
        UserId = feedback.UserId,
        Category = feedback.Category,
        Subject = feedback.Subject,
        Content = feedback.Content,
        Rating = feedback.Rating,
        ContactEmail = feedback.ContactEmail,
        Status = feedback.Status,
        CreatedAt = feedback.CreatedAt
    };
}
