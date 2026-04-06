namespace Verendar.Identity.Application.Dtos;

public record IdentityOverviewStatsResponse(
    IdentityUserStatsSection Users,
    IdentityFeedbackStatsSection Feedback);

public record IdentityUserStatsSection(
    int Total,
    int EmailVerified,
    Dictionary<string, int> ByRole);

public record IdentityFeedbackStatsSection(
    int Total,
    Dictionary<string, int> ByStatus,
    Dictionary<string, int> ByCategory,
    double? AvgRating);
