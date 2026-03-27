using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;
using Verendar.Garage.Domain.Enums;
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

    // ─── Request normalization ────────────────────────────────────────────────

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
