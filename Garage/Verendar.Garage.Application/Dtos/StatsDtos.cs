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
