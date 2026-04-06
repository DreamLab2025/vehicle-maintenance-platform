using Verendar.Identity.Domain.Enums;

namespace Verendar.Identity.Application.Dtos;

public record CreateFeedbackRequest
{
    public FeedbackCategory Category { get; init; }
    public string Subject { get; init; } = null!;
    public string Content { get; init; } = null!;
    public int? Rating { get; init; }
    public string? ContactEmail { get; init; }
    public List<string>? ImageUrls { get; init; }
}

public record UpdateFeedbackStatusRequest
{
    public FeedbackStatus Status { get; init; }
}

public record FeedbackResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public FeedbackCategory Category { get; init; }
    public string Subject { get; init; } = null!;
    public string Content { get; init; } = null!;
    public int? Rating { get; init; }
    public string? ContactEmail { get; init; }
    public List<string> ImageUrls { get; init; } = [];
    public FeedbackStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
