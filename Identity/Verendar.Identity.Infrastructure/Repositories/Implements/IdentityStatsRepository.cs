using Verendar.Identity.Domain.Enums;
using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Infrastructure.Repositories.Implements;

public class IdentityStatsRepository(UserDbContext context) : IIdentityStatsRepository
{
    public async Task<UserStatsRaw> GetUserStatsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var users = await context.Users
            .Where(u => u.CreatedAt >= from && u.CreatedAt <= to)
            .Select(u => new { u.EmailVerified, u.Roles })
            .ToListAsync(ct);

        return new UserStatsRaw(
            Total: users.Count,
            EmailVerified: users.Count(u => u.EmailVerified),
            RoleUser: users.Count(u => u.Roles.Contains(UserRole.User)),
            RoleAdmin: users.Count(u => u.Roles.Contains(UserRole.Admin)),
            RoleGarageOwner: users.Count(u => u.Roles.Contains(UserRole.GarageOwner)),
            RoleMechanic: users.Count(u => u.Roles.Contains(UserRole.Mechanic)),
            RoleGarageManager: users.Count(u => u.Roles.Contains(UserRole.GarageManager))
        );
    }

    public async Task<FeedbackStatsRaw> GetFeedbackStatsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var feedbacks = await context.Feedbacks
            .Where(f => f.CreatedAt >= from && f.CreatedAt <= to)
            .Select(f => new { f.Status, f.Category, f.Rating })
            .ToListAsync(ct);

        var avgRating = feedbacks
            .Where(f => f.Rating.HasValue)
            .Average(f => (double?)f.Rating);

        return new FeedbackStatsRaw(
            Total: feedbacks.Count,
            StatusPending: feedbacks.Count(f => f.Status == FeedbackStatus.Pending),
            StatusReviewed: feedbacks.Count(f => f.Status == FeedbackStatus.Reviewed),
            StatusResolved: feedbacks.Count(f => f.Status == FeedbackStatus.Resolved),
            CategoryGeneral: feedbacks.Count(f => f.Category == FeedbackCategory.General),
            CategoryBug: feedbacks.Count(f => f.Category == FeedbackCategory.Bug),
            CategoryFeature: feedbacks.Count(f => f.Category == FeedbackCategory.Feature),
            CategoryUX: feedbacks.Count(f => f.Category == FeedbackCategory.UX),
            CategoryPerformance: feedbacks.Count(f => f.Category == FeedbackCategory.Performance),
            CategoryOther: feedbacks.Count(f => f.Category == FeedbackCategory.Other),
            AvgRating: avgRating.HasValue ? Math.Round(avgRating.Value, 1) : null
        );
    }

    public async Task<List<(string Period, int Count)>> GetUserGrowthAsync(
        DateTime from, DateTime to, string groupBy, CancellationToken ct = default)
    {
        var dates = await context.Users
            .Where(u => u.CreatedAt >= from && u.CreatedAt <= to)
            .Select(u => u.CreatedAt)
            .ToListAsync(ct);

        return groupBy == "day"
            ? dates
                .GroupBy(d => d.ToString("yyyy-MM-dd"))
                .Select(g => (Period: g.Key, Count: g.Count()))
                .ToList()
            : dates
                .GroupBy(d => d.ToString("yyyy-MM"))
                .Select(g => (Period: g.Key, Count: g.Count()))
                .ToList();
    }
}
