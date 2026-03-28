namespace Verendar.Ai.Application.Clients
{
    public interface IVehicleServiceClient
    {
        Task<ApiResponse<VehicleServiceUserVehicleResponse>> GetUserVehicleByIdAsync(
            Guid userVehicleId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<VehicleServiceDefaultScheduleResponse>> GetDefaultScheduleAsync(
            Guid vehicleModelId,
            string partCategorySlug,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<VehicleServiceOdometerSummaryResponse>> GetOdometerSummaryAsync(
            Guid userVehicleId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<List<VehicleServiceBaselinePartItem>>> GetBaselinePartsAsync(
            Guid userVehicleId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<object>> ApplyTrackingInternalAsync(
            Guid vehicleId,
            Guid userId,
            VehicleServiceApplyTrackingRequest request,
            CancellationToken cancellationToken = default);
    }
}
