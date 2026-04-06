using Verendar.Common.Stats;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Application.Services.Implements;

public class IdentityStatsService(IUnitOfWork unitOfWork) : IIdentityStatsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<IdentityOverviewStatsResponse>> GetOverviewAsync(
        DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var toDate = to ?? DateOnly.FromDateTime(today);
        var fromDate = from ?? toDate.AddMonths(-12);

        var fromDt = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt = toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var u = await _unitOfWork.Stats.GetUserStatsAsync(fromDt, toDt, ct);
        var f = await _unitOfWork.Stats.GetFeedbackStatsAsync(fromDt, toDt, ct);

        var response = new IdentityOverviewStatsResponse(
            Users: new IdentityUserStatsSection(
                Total: u.Total,
                EmailVerified: u.EmailVerified,
                ByRole: new Dictionary<string, int>
                {
                    ["user"] = u.RoleUser,
                    ["admin"] = u.RoleAdmin,
                    ["garageOwner"] = u.RoleGarageOwner,
                    ["mechanic"] = u.RoleMechanic,
                    ["garageManager"] = u.RoleGarageManager,
                }),
            Feedback: new IdentityFeedbackStatsSection(
                Total: f.Total,
                ByStatus: new Dictionary<string, int>
                {
                    ["pending"] = f.StatusPending,
                    ["reviewed"] = f.StatusReviewed,
                    ["resolved"] = f.StatusResolved,
                },
                ByCategory: new Dictionary<string, int>
                {
                    ["general"] = f.CategoryGeneral,
                    ["bug"] = f.CategoryBug,
                    ["feature"] = f.CategoryFeature,
                    ["ux"] = f.CategoryUX,
                    ["performance"] = f.CategoryPerformance,
                    ["other"] = f.CategoryOther,
                },
                AvgRating: f.AvgRating
            )
        );

        return ApiResponse<IdentityOverviewStatsResponse>.SuccessResponse(
            response, "Lấy thống kê identity thành công.");
    }

    public async Task<ApiResponse<ChartTimelineResponse>> GetUserGrowthAsync(
        ChartQueryRequest request, CancellationToken ct = default)
    {
        var validationError = request.Validate();
        if (validationError is not null)
            return ApiResponse<ChartTimelineResponse>.FailureResponse(validationError, 400);

        var (fromDt, toDt, groupBy) = request.Normalize();

        var rawPoints = await _unitOfWork.Stats.GetUserGrowthAsync(fromDt, toDt, groupBy, ct);
        var lookup = rawPoints.ToDictionary(p => p.Period, p => p.Count);

        var allPeriods = GeneratePeriods(fromDt, toDt, groupBy);
        var points = allPeriods
            .Select(p => new ChartPoint(p, lookup.GetValueOrDefault(p, 0)))
            .ToList();

        var fromDate = request.From ?? DateOnly.FromDateTime(fromDt);
        var toDate = request.To ?? DateOnly.FromDateTime(toDt);

        var response = new ChartTimelineResponse(groupBy, fromDate, toDate, points);

        return ApiResponse<ChartTimelineResponse>.SuccessResponse(
            response, "Lấy biểu đồ tăng trưởng người dùng thành công.");
    }

    private static List<string> GeneratePeriods(DateTime from, DateTime to, string groupBy)
    {
        var periods = new List<string>();

        if (groupBy == "day")
        {
            var current = from.Date;
            while (current <= to.Date)
            {
                periods.Add(current.ToString("yyyy-MM-dd"));
                current = current.AddDays(1);
            }
        }
        else
        {
            var current = new DateTime(from.Year, from.Month, 1);
            var end = new DateTime(to.Year, to.Month, 1);
            while (current <= end)
            {
                periods.Add(current.ToString("yyyy-MM"));
                current = current.AddMonths(1);
            }
        }

        return periods;
    }
}
