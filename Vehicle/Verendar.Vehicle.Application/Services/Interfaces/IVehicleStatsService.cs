using Verendar.Common.Stats;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleStatsService
    {
        Task<ApiResponse<VehicleOverviewStatsResponse>> GetOverviewStatsAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
        Task<ApiResponse<ChartTimelineResponse>> GetMaintenanceActivityChartAsync(ChartQueryRequest request, CancellationToken ct = default);
    }
}
