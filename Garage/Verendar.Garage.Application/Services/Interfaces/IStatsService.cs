using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IStatsService
{
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
}
