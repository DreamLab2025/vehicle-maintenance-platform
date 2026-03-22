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
    }
}
