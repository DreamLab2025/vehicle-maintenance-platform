using Verendar.Identity.Domain.Enums;

namespace Verendar.Identity.Domain.Entities;

[Index(nameof(UserId))]
[Index(nameof(Status))]
public class Feedback : BaseEntity
{
    public Guid UserId { get; set; }

    public FeedbackCategory Category { get; set; }

    [MaxLength(200)]
    public string Subject { get; set; } = null!;

    [MaxLength(5000)]
    public string Content { get; set; } = null!;

    public int? Rating { get; set; }

    [MaxLength(256)]
    public string? ContactEmail { get; set; }

    public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
}
