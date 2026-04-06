using Verendar.Common.Stats;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;
using Verendar.Garage.Domain.Models;

namespace Verendar.Garage.Application.Services.Implements;

public class StatsService(ILogger<StatsService> logger, IUnitOfWork unitOfWork) : IStatsService
{
    private readonly ILogger<StatsService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private const string Currency = "VND";

    public async Task<ApiResponse<GarageStatsResponse>> GetGarageStatsAsync(
        Guid garageId,
        Guid actorId,
        StatsRequest request,
        CancellationToken ct = default)
    {
        var (from, to, period) = NormalizeRequest(request);

        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.Id == garageId, ct);
        if (garage is null)
            return ApiResponse<GarageStatsResponse>.NotFoundResponse(EndpointMessages.Stats.GarageNotFound);

        if (garage.OwnerId != actorId)
            return ApiResponse<GarageStatsResponse>.ForbiddenResponse(EndpointMessages.Stats.Forbidden);

        var branchIds = garage.Branches
            .Where(b => b.DeletedAt == null)
            .Select(b => b.Id)
            .ToList();

        if (branchIds.Count == 0)
        {
            var empty = new GarageStatsResponse(
                Revenue: EmptyRevenue(),
                Bookings: EmptyBookingCount(),
                Branches: []);
            return ApiResponse<GarageStatsResponse>.SuccessResponse(empty, EndpointMessages.Stats.GarageStatsSuccess);
        }

        var revenueTask = _unitOfWork.Bookings.GetRevenueStatsAsync(branchIds, from, to, period, ct);
        var countTask = _unitOfWork.Bookings.GetBookingCountsByStatusAsync(branchIds, from, to, ct);
        var summaryTask = _unitOfWork.Bookings.GetBranchSummariesAsync(branchIds, from, to, ct);
        var ratingsTask = _unitOfWork.Reviews.GetBulkRatingSummaryAsync(branchIds, ct);

        await Task.WhenAll(revenueTask, countTask, summaryTask, ratingsTask);

        var revenue = revenueTask.Result;
        var counts = countTask.Result;
        var summaries = summaryTask.Result;
        var ratings = ratingsTask.Result;

        var branchNameById = garage.Branches
            .Where(b => b.DeletedAt == null)
            .ToDictionary(b => b.Id, b => b.Name);

        var branches = summaries.Select(s =>
        {
            branchNameById.TryGetValue(s.BranchId, out var name);
            ratings.TryGetValue(s.BranchId, out var rating);
            var total = CountTotal(s.Counts);
            return new BranchStatsItemDto(
                BranchId: s.BranchId,
                BranchName: name ?? string.Empty,
                Revenue: MapRevenueByStatus(s.Revenue),
                BookingCount: total,
                CompletedCount: s.Counts.Completed,
                AverageRating: rating.AverageRating,
                ReviewCount: rating.ReviewCount
            );
        }).ToList();

        var response = new GarageStatsResponse(
            Revenue: MapRevenueStats(revenue),
            Bookings: MapBookingCount(counts),
            Branches: branches
        );

        _logger.LogInformation("GetGarageStats: garageId={GarageId} from={From} to={To}", garageId, from, to);
        return ApiResponse<GarageStatsResponse>.SuccessResponse(response, EndpointMessages.Stats.GarageStatsSuccess);
    }

    public async Task<ApiResponse<BranchStatsResponse>> GetBranchStatsAsync(
        Guid garageId,
        Guid branchId,
        Guid actorId,
        StatsRequest request,
        CancellationToken ct = default)
    {
        var (from, to, period) = NormalizeRequest(request);

        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.Id == garageId, ct);
        if (garage is null)
            return ApiResponse<BranchStatsResponse>.NotFoundResponse(EndpointMessages.Stats.GarageNotFound);

        var branch = garage.Branches.FirstOrDefault(b => b.Id == branchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<BranchStatsResponse>.NotFoundResponse(EndpointMessages.Stats.BranchNotFound);

        var isOwner = garage.OwnerId == actorId;
        if (!isOwner)
        {
            var isActiveManager = await _unitOfWork.Members.IsActiveManagerOfBranchAsync(branchId, actorId, ct);
            if (!isActiveManager)
                return ApiResponse<BranchStatsResponse>.ForbiddenResponse(EndpointMessages.Stats.Forbidden);
        }

        var branchIds = new List<Guid> { branchId };

        var revenueTask = _unitOfWork.Bookings.GetRevenueStatsAsync(branchIds, from, to, period, ct);
        var countTask = _unitOfWork.Bookings.GetBookingCountsByStatusAsync(branchIds, from, to, ct);
        var topItemsTask = _unitOfWork.Bookings.GetTopItemsAsync(branchId, from, to, limit: 10, ct);
        var mechanicsTask = _unitOfWork.Bookings.GetMechanicStatsAsync(branchId, from, to, ct);
        var ratingTask = _unitOfWork.Reviews.GetRatingSummaryAsync(branchId, ct);

        await Task.WhenAll(revenueTask, countTask, topItemsTask, mechanicsTask, ratingTask);

        var revenue = revenueTask.Result;
        var counts = countTask.Result;
        var topItems = topItemsTask.Result;
        var mechanics = mechanicsTask.Result;
        var rating = ratingTask.Result;

        var response = new BranchStatsResponse(
            Revenue: MapRevenueStats(revenue),
            Bookings: MapBookingCount(counts),
            Reviews: new ReviewSummaryDto(rating.AverageRating, rating.ReviewCount),
            TopItems: topItems.Select(MapTopItem).ToList(),
            Mechanics: mechanics.Select(MapMechanic).ToList()
        );

        _logger.LogInformation("GetBranchStats: branchId={BranchId} from={From} to={To}", branchId, from, to);
        return ApiResponse<BranchStatsResponse>.SuccessResponse(response, EndpointMessages.Stats.BranchStatsSuccess);
    }

    // ─── Mapping helpers ─────────────────────────────────────────────────────

    private static RevenueDto MapRevenueStats(RevenueStats stats)
    {
        var changePercent = stats.PreviousCompletedTotal > 0
            ? Math.Round(
                (stats.ByStatus.Completed - stats.PreviousCompletedTotal) / stats.PreviousCompletedTotal * 100,
                2)
            : (decimal?)null;

        return new RevenueDto(
            ByStatus: MapRevenueByStatus(stats.ByStatus),
            ChangePercent: changePercent,
            Trend: stats.Trend.Select(t => new TrendPointDto(
                Label: t.Label,
                Completed: t.Completed,
                Cancelled: t.Cancelled,
                Other: t.Other,
                Currency: Currency
            )).ToList()
        );
    }

    private static RevenueByStatusDto MapRevenueByStatus(RevenueByStatus r) =>
        new(r.Completed, r.InProgress, r.Confirmed, r.AwaitingConfirmation, r.Pending, r.Cancelled, Currency);

    private static BookingCountDto MapBookingCount(BookingCountByStatus c)
    {
        var total = CountTotal(c);
        var rate = total > 0 ? Math.Round((decimal)c.Completed / total, 3) : 0m;
        return new BookingCountDto(
            Total: total,
            ByStatus: new BookingStatusCountDto(c.Completed, c.InProgress, c.Confirmed, c.AwaitingConfirmation, c.Pending, c.Cancelled),
            CompletionRate: rate
        );
    }

    private static TopItemDto MapTopItem(TopItemStats t) =>
        new(t.ItemName, t.ItemType, t.BookingCount, t.CompletedRevenue, t.CancelledRevenue, Currency);

    private static MechanicStatDto MapMechanic(MechanicStats m) =>
        new(m.MemberId, m.DisplayName, m.CompletedBookings, m.CompletedRevenue, Currency);

    private static int CountTotal(BookingCountByStatus c) =>
        c.Completed + c.InProgress + c.Confirmed + c.AwaitingConfirmation + c.Pending + c.Cancelled;

    private static RevenueDto EmptyRevenue() =>
        new(new RevenueByStatusDto(0, 0, 0, 0, 0, 0, Currency), null, []);

    private static BookingCountDto EmptyBookingCount() =>
        new(0, new BookingStatusCountDto(0, 0, 0, 0, 0, 0), 0);

    public async Task<ApiResponse<GarageOverviewStatsResponse>> GetPlatformOverviewStatsAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken ct = default)
    {
        var (fromDt, toDt) = NormalizeRange(from, to);

        var garageCountsTask = _unitOfWork.Garages.GetStatusCountsAsync(fromDt, toDt, ct);
        var branchCountsTask = _unitOfWork.GarageBranches.GetBranchCountsAsync(ct);
        var bookingCountsTask = _unitOfWork.Bookings.GetAllBookingCountsByStatusAsync(fromDt, toDt, ct);
        var revenueTask = _unitOfWork.Bookings.GetCompletedRevenueAsync(null, fromDt, toDt, ct);
        var reviewsTask = _unitOfWork.Reviews.GetGlobalRatingSummaryAsync(fromDt, toDt, ct);

        await Task.WhenAll(garageCountsTask, branchCountsTask, bookingCountsTask, revenueTask, reviewsTask);

        var garageCounts = garageCountsTask.Result;
        var branchCounts = branchCountsTask.Result;
        var bookingCounts = bookingCountsTask.Result;
        var revenue = revenueTask.Result;
        var reviews = reviewsTask.Result;

        var totalBookings = CountTotal(bookingCounts);
        var completionRate = totalBookings > 0 ? Math.Round((decimal)bookingCounts.Completed / totalBookings * 100, 2) : 0m;
        var cancellationRate = totalBookings > 0 ? Math.Round((decimal)bookingCounts.Cancelled / totalBookings * 100, 2) : 0m;

        var response = new GarageOverviewStatsResponse(
            Garages: new GaragesStatsSummaryDto(
                Total: garageCounts.Total,
                ByStatus: new GarageStatusCountsDto(
                    Pending: garageCounts.Pending,
                    Active: garageCounts.Active,
                    Suspended: garageCounts.Suspended,
                    Rejected: garageCounts.Rejected)),
            Branches: new BranchesStatsSummaryDto(
                Total: branchCounts.Total,
                Active: branchCounts.Active,
                Inactive: branchCounts.Inactive),
            Bookings: new BookingsOverviewDto(
                Total: totalBookings,
                ByStatus: new BookingStatusCountDto(
                    bookingCounts.Completed, bookingCounts.InProgress, bookingCounts.Confirmed,
                    bookingCounts.AwaitingConfirmation, bookingCounts.Pending, bookingCounts.Cancelled),
                CompletionRate: completionRate,
                CancellationRate: cancellationRate),
            Revenue: new RevenueOverviewDto(Total: revenue, Currency: Currency),
            Reviews: new ReviewsOverviewDto(AvgRating: reviews.AvgRating, TotalCount: reviews.TotalCount)
        );

        _logger.LogInformation("GetPlatformOverviewStats: from={From} to={To}", fromDt, toDt);
        return ApiResponse<GarageOverviewStatsResponse>.SuccessResponse(response, EndpointMessages.Stats.PlatformStatsSuccess);
    }

    public async Task<ApiResponse<GarageDetailStatsResponse>> GetGarageDetailStatsAsync(
        Guid garageId,
        Guid actorId,
        bool isAdmin,
        DateOnly? from,
        DateOnly? to,
        CancellationToken ct = default)
    {
        var (fromDt, toDt) = NormalizeRange(from, to);

        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.Id == garageId, ct);
        if (garage is null)
            return ApiResponse<GarageDetailStatsResponse>.NotFoundResponse(EndpointMessages.Stats.GarageNotFound);

        if (!isAdmin && garage.OwnerId != actorId)
            return ApiResponse<GarageDetailStatsResponse>.ForbiddenResponse(EndpointMessages.Stats.Forbidden);

        var branchIds = garage.Branches
            .Where(b => b.DeletedAt == null)
            .Select(b => b.Id)
            .ToList();

        if (branchIds.Count == 0)
        {
            var empty = new GarageDetailStatsResponse(
                GarageId: garageId,
                BusinessName: garage.BusinessName,
                TotalBranches: 0,
                Bookings: new BookingsOverviewDto(0, new BookingStatusCountDto(0, 0, 0, 0, 0, 0), 0, 0),
                Revenue: new RevenueOverviewDto(0, Currency),
                Reviews: new GarageDetailReviewsDto(0, 0, new Dictionary<int, int>()),
                TopBranches: [],
                TopServices: []);
            return ApiResponse<GarageDetailStatsResponse>.SuccessResponse(empty, EndpointMessages.Stats.GarageDetailStatsSuccess);
        }

        var countTask = _unitOfWork.Bookings.GetBookingCountsByStatusAsync(branchIds, fromDt, toDt, ct);
        var revenueTask = _unitOfWork.Bookings.GetCompletedRevenueAsync(branchIds, fromDt, toDt, ct);
        var ratingTask = _unitOfWork.Reviews.GetRatingSummaryAsync(branchIds.First(), ct);
        var bulkRatingTask = _unitOfWork.Reviews.GetBulkRatingSummaryAsync(branchIds, ct);
        var ratingDistTask = _unitOfWork.Reviews.GetRatingDistributionAsync(branchIds, ct);
        var branchCountsTask = _unitOfWork.Bookings.GetBranchBookingCountsAsync(branchIds, fromDt, toDt, ct);
        var topServicesTask = _unitOfWork.Bookings.GetTopServicesAsync(branchIds, fromDt, toDt, limit: 5, ct);

        await Task.WhenAll(countTask, revenueTask, ratingDistTask, bulkRatingTask, branchCountsTask, topServicesTask);

        var counts = countTask.Result;
        var revenue = revenueTask.Result;
        var bulkRatings = bulkRatingTask.Result;
        var ratingDist = ratingDistTask.Result;
        var branchBookingCounts = branchCountsTask.Result;
        var topServices = topServicesTask.Result;

        // Aggregate global rating for the garage from per-branch ratings
        var allReviews = bulkRatings.Values.ToList();
        var totalReviewCount = allReviews.Sum(r => r.ReviewCount);
        var avgRating = totalReviewCount > 0
            ? allReviews.Sum(r => r.AverageRating * r.ReviewCount) / totalReviewCount
            : 0;

        var totalBookings = CountTotal(counts);
        var completionRate = totalBookings > 0 ? Math.Round((decimal)counts.Completed / totalBookings * 100, 2) : 0m;
        var cancellationRate = totalBookings > 0 ? Math.Round((decimal)counts.Cancelled / totalBookings * 100, 2) : 0m;

        var branchNameById = garage.Branches
            .Where(b => b.DeletedAt == null)
            .ToDictionary(b => b.Id, b => b.Name);

        var topBranches = branchBookingCounts
            .OrderByDescending(b => b.BookingCount)
            .Take(5)
            .Select(b => new TopBranchDto(
                BranchId: b.BranchId,
                Name: branchNameById.GetValueOrDefault(b.BranchId, string.Empty),
                BookingCount: b.BookingCount,
                Revenue: b.CompletedRevenue))
            .ToList();

        var response = new GarageDetailStatsResponse(
            GarageId: garageId,
            BusinessName: garage.BusinessName,
            TotalBranches: branchIds.Count,
            Bookings: new BookingsOverviewDto(
                Total: totalBookings,
                ByStatus: new BookingStatusCountDto(
                    counts.Completed, counts.InProgress, counts.Confirmed,
                    counts.AwaitingConfirmation, counts.Pending, counts.Cancelled),
                CompletionRate: completionRate,
                CancellationRate: cancellationRate),
            Revenue: new RevenueOverviewDto(Total: revenue, Currency: Currency),
            Reviews: new GarageDetailReviewsDto(
                AvgRating: Math.Round(avgRating, 2),
                TotalCount: totalReviewCount,
                ByRating: ratingDist),
            TopBranches: topBranches,
            TopServices: topServices.Select(s => new TopServiceDto(s.ServiceId, s.ServiceName, s.BookingCount)).ToList()
        );

        _logger.LogInformation("GetGarageDetailStats: garageId={GarageId} from={From} to={To}", garageId, fromDt, toDt);
        return ApiResponse<GarageDetailStatsResponse>.SuccessResponse(response, EndpointMessages.Stats.GarageDetailStatsSuccess);
    }

    public async Task<ApiResponse<ChartTimelineResponse>> GetBookingTrafficChartAsync(
        ChartQueryRequest request,
        CancellationToken ct = default)
    {
        var error = request.Validate();
        if (error is not null)
            return ApiResponse<ChartTimelineResponse>.FailureResponse(error);

        var (from, to, groupBy) = request.Normalize();
        var points = await _unitOfWork.Bookings.GetBookingTrafficChartAsync(from, to, groupBy, ct);

        var fromDate = request.From ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var toDate = request.To ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var response = new ChartTimelineResponse(groupBy, fromDate, toDate, points);
        return ApiResponse<ChartTimelineResponse>.SuccessResponse(response, EndpointMessages.Stats.ChartSuccess);
    }

    public async Task<ApiResponse<ChartComparisonResponse>> GetBookingOutcomesChartAsync(
        ChartQueryRequest request,
        CancellationToken ct = default)
    {
        var error = request.Validate();
        if (error is not null)
            return ApiResponse<ChartComparisonResponse>.FailureResponse(error);

        var (from, to, groupBy) = request.Normalize();
        var (labels, completedData, cancelledData) = await _unitOfWork.Bookings.GetBookingOutcomesChartAsync(from, to, groupBy, ct);

        var fromDate = request.From ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var toDate = request.To ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var response = new ChartComparisonResponse(
            GroupBy: groupBy,
            From: fromDate,
            To: toDate,
            Labels: labels,
            Series:
            [
                new ChartSeries("Completed", completedData),
                new ChartSeries("Cancelled", cancelledData)
            ]);

        return ApiResponse<ChartComparisonResponse>.SuccessResponse(response, EndpointMessages.Stats.ChartSuccess);
    }

    public async Task<ApiResponse<RevenueChartResponse>> GetGarageRevenueChartAsync(
        Guid garageId,
        Guid actorId,
        bool isAdmin,
        ChartQueryRequest request,
        CancellationToken ct = default)
    {
        var error = request.Validate();
        if (error is not null)
            return ApiResponse<RevenueChartResponse>.FailureResponse(error);

        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.Id == garageId, ct);
        if (garage is null)
            return ApiResponse<RevenueChartResponse>.NotFoundResponse(EndpointMessages.Stats.GarageNotFound);

        if (!isAdmin && garage.OwnerId != actorId)
            return ApiResponse<RevenueChartResponse>.ForbiddenResponse(EndpointMessages.Stats.Forbidden);

        var branchIds = garage.Branches
            .Where(b => b.DeletedAt == null)
            .Select(b => b.Id)
            .ToList();

        var (from, to, groupBy) = request.Normalize();
        var fromDate = request.From ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var toDate = request.To ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var points = branchIds.Count == 0
            ? []
            : await _unitOfWork.Bookings.GetRevenueChartAsync(branchIds, from, to, groupBy, ct);

        var response = new RevenueChartResponse(groupBy, fromDate, toDate, Currency, points);
        return ApiResponse<RevenueChartResponse>.SuccessResponse(response, EndpointMessages.Stats.ChartSuccess);
    }

    // ─── Request normalization ────────────────────────────────────────────────

    private static (DateTime From, DateTime To) NormalizeRange(DateOnly? from, DateOnly? to)
    {
        var today = DateTime.UtcNow.Date;
        var toDate = to ?? DateOnly.FromDateTime(today);
        var fromDate = from ?? toDate.AddMonths(-12);
        return (
            fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc)
        );
    }

    private static (DateTime From, DateTime To, StatsPeriod Period) NormalizeRequest(StatsRequest request)
    {
        var today = DateTime.UtcNow.Date;

        var from = request.From.HasValue
            ? request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            : new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var to = request.To.HasValue
            ? request.To.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc)
            : today.AddDays(1).AddTicks(-1).ToUniversalTime();

        var period = request.Period?.ToLowerInvariant() switch
        {
            "weekly" => StatsPeriod.Weekly,
            "quarterly" => StatsPeriod.Quarterly,
            _ => StatsPeriod.Monthly
        };

        return (from, to, period);
    }
}
