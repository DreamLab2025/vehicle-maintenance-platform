namespace Verendar.Identity.Domain.Repositories.Interfaces;

public interface IIdentityStatsRepository
{
    Task<UserStatsRaw> GetUserStatsAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<FeedbackStatsRaw> GetFeedbackStatsAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<(string Period, int Count)>> GetUserGrowthAsync(DateTime from, DateTime to, string groupBy, CancellationToken ct = default);
}

public record UserStatsRaw(
    int Total,
    int EmailVerified,
    int RoleUser,
    int RoleAdmin,
    int RoleGarageOwner,
    int RoleMechanic,
    int RoleGarageManager);

public record FeedbackStatsRaw(
    int Total,
    int StatusPending,
    int StatusReviewed,
    int StatusResolved,
    int CategoryGeneral,
    int CategoryBug,
    int CategoryFeature,
    int CategoryUX,
    int CategoryPerformance,
    int CategoryOther,
    double? AvgRating);
