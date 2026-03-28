using Verendar.Vehicle.Application.Dtos.Internal;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IPartTrackingService
    {
        Task<ApiResponse<List<PartSummary>>> GetPartsByUserVehicleAsync(Guid userId, Guid userVehicleId);
        Task<ApiResponse<List<TrackingCycleSummary>>> GetCyclesForPartAsync(Guid userId, Guid vehicleId, Guid partTrackingId);
        Task<ApiResponse<PartTrackingSummary>> ApplyTrackingConfigAsync(Guid userId, Guid vehicleId, ApplyTrackingConfigRequest request);
        Task InitializeForVehicleAsync(Guid userVehicleId, Guid vehicleModelId);
        Task<ApiResponse<List<BaselinePartItemDto>>> GetBaselinePartsAsync(Guid userVehicleId, CancellationToken cancellationToken = default);
    }
}
