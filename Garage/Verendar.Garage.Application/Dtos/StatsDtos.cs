using Verendar.Common.Stats;

namespace Verendar.Garage.Application.Dtos;

// ─── Request ───────────────────────────────────────────────────────────────

public record StatsRequest(DateOnly? From, DateOnly? To, string? Period);

// ─── Shared building blocks ─────────────────────────────────────────────────

public record RevenueByStatusDto(
    decimal Completed,
    decimal InProgress,
    decimal Confirmed,
    decimal AwaitingConfirmation,
    decimal Pending,
    decimal Cancelled,
    string Currency);

public record TrendPointDto(
    string Label,
    decimal Completed,
    decimal Cancelled,
    decimal Other,
    string Currency);

public record RevenueDto(
    RevenueByStatusDto ByStatus,
    decimal? ChangePercent,
    List<TrendPointDto> Trend);

public record BookingStatusCountDto(
    int Completed,
    int InProgress,
    int Confirmed,
    int AwaitingConfirmation,
    int Pending,
    int Cancelled);

public record BookingCountDto(int Total, BookingStatusCountDto ByStatus, decimal CompletionRate);

public record ReviewSummaryDto(double AverageRating, int TotalCount);

// ─── Garage-level response ──────────────────────────────────────────────────

public record BranchStatsItemDto(
    Guid BranchId,
    string BranchName,
    RevenueByStatusDto Revenue,
    int BookingCount,
    int CompletedCount,
    double AverageRating,
    int ReviewCount);

public record GarageStatsResponse(
    RevenueDto Revenue,
    BookingCountDto Bookings,
    List<BranchStatsItemDto> Branches);

// ─── Branch-level response ──────────────────────────────────────────────────

public record TopItemDto(
    string ItemName,
    string ItemType,
    int BookingCount,
    decimal CompletedRevenue,
    decimal CancelledRevenue,
    string Currency);

public record MechanicStatDto(
    Guid MemberId,
    string DisplayName,
    int CompletedBookings,
    decimal Revenue,
    string Currency);

public record BranchStatsResponse(
    RevenueDto Revenue,
    BookingCountDto Bookings,
    ReviewSummaryDto Reviews,
    List<TopItemDto> TopItems,
    List<MechanicStatDto> Mechanics);

// ─── Admin platform overview ─────────────────────────────────────────────────

public record GarageOverviewStatsResponse(
    GaragesStatsSummaryDto Garages,
    BranchesStatsSummaryDto Branches,
    BookingsOverviewDto Bookings,
    RevenueOverviewDto Revenue,
    ReviewsOverviewDto Reviews);

public record GarageStatusCountsDto(int Pending, int Active, int Suspended, int Rejected);

public record GaragesStatsSummaryDto(int Total, GarageStatusCountsDto ByStatus);

public record BranchesStatsSummaryDto(int Total, int Active, int Inactive);

public record BookingsOverviewDto(
    int Total,
    BookingStatusCountDto ByStatus,
    decimal CompletionRate,
    decimal CancellationRate);

public record RevenueOverviewDto(decimal Total, string Currency);

public record ReviewsOverviewDto(double AvgRating, int TotalCount);

// ─── Garage detail stats (Owner | Admin) ────────────────────────────────────

public record GarageDetailStatsResponse(
    Guid GarageId,
    string BusinessName,
    int TotalBranches,
    BookingsOverviewDto Bookings,
    RevenueOverviewDto Revenue,
    GarageDetailReviewsDto Reviews,
    List<TopBranchDto> TopBranches,
    List<TopServiceDto> TopServices);

public record GarageDetailReviewsDto(
    double AvgRating,
    int TotalCount,
    Dictionary<int, int> ByRating);

public record TopBranchDto(Guid BranchId, string Name, int BookingCount, decimal Revenue);

public record TopServiceDto(Guid ServiceId, string Name, int BookingCount);

// ─── Revenue chart (with currency) ──────────────────────────────────────────

public record RevenueChartResponse(
    string GroupBy,
    DateOnly From,
    DateOnly To,
    string Currency,
    List<ChartPoint> Points);
