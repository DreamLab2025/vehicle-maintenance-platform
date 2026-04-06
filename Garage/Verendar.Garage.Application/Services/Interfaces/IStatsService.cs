using Verendar.Common.Stats;
using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IStatsService
{
    // ─── Existing (garage owner / branch) ────────────────────────────────

    Task<ApiResponse<GarageStatsResponse>> GetGarageStatsAsync(
        Guid garageId,
        Guid actorId,
        StatsRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<BranchStatsResponse>> GetBranchStatsAsync(
        Guid garageId,
        Guid branchId,
        Guid actorId,
        StatsRequest request,
        CancellationToken ct = default);

    // ─── New (admin platform + charts) ───────────────────────────────────

    Task<ApiResponse<GarageOverviewStatsResponse>> GetPlatformOverviewStatsAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken ct = default);

    Task<ApiResponse<GarageDetailStatsResponse>> GetGarageDetailStatsAsync(
        Guid garageId,
        Guid actorId,
        bool isAdmin,
        DateOnly? from,
        DateOnly? to,
        CancellationToken ct = default);

    Task<ApiResponse<ChartTimelineResponse>> GetBookingTrafficChartAsync(
        ChartQueryRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<ChartComparisonResponse>> GetBookingOutcomesChartAsync(
        ChartQueryRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<RevenueChartResponse>> GetGarageRevenueChartAsync(
        Guid garageId,
        Guid actorId,
        bool isAdmin,
        ChartQueryRequest request,
        CancellationToken ct = default);
}
