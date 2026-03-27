namespace Verendar.Garage.Domain.Models;

public record RevenueByStatus(
    decimal Completed,
    decimal InProgress,
    decimal Confirmed,
    decimal AwaitingConfirmation,
    decimal Pending,
    decimal Cancelled);

public record TrendPoint(string Label, decimal Completed, decimal Cancelled, decimal Other);

public record RevenueStats(RevenueByStatus ByStatus, decimal PreviousCompletedTotal, List<TrendPoint> Trend);

public record TopItemStats(
    Guid? ProductId,
    Guid? ServiceId,
    Guid? BundleId,
    string ItemName,
    string ItemType,
    int BookingCount,
    decimal CompletedRevenue,
    decimal CancelledRevenue);

public record MechanicStats(Guid MemberId, string DisplayName, int CompletedBookings, decimal CompletedRevenue);

public record BookingCountByStatus(
    int Completed,
    int InProgress,
    int Confirmed,
    int AwaitingConfirmation,
    int Pending,
    int Cancelled);

public record BranchBookingSummary(Guid BranchId, RevenueByStatus Revenue, BookingCountByStatus Counts);
