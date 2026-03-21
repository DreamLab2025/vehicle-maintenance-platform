namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IPartTrackingService
    {
        Task<ApiResponse<List<PartSummary>>> GetPartsByUserVehicleAsync(Guid userId, Guid userVehicleId);
        Task<ApiResponse<PartTrackingSummary>> ApplyTrackingConfigAsync(Guid userId, Guid vehicleId, ApplyTrackingConfigRequest request);
        Task InitializeForVehicleAsync(Guid userVehicleId, Guid vehicleModelId);
    }
}
