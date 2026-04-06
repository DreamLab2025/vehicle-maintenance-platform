using Verendar.Common.Shared;
using Verendar.Common.Stats;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleStatsService
    {
        Task<ApiResponse<VehicleOverviewStatsResponse>> GetOverviewStatsAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
        Task<ApiResponse<ChartTimelineResponse>> GetMaintenanceActivityChartAsync(ChartQueryRequest request, CancellationToken ct = default);
    }
}
