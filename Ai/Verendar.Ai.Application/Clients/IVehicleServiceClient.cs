using Verendar.Ai.Application.Dtos.VehicleService;
namespace Verendar.Ai.Application.Clients
{
    public interface IVehicleServiceClient
    {
        Task<ApiResponse<VehicleServiceUserVehicleResponse>> GetUserVehicleByIdAsync(
            Guid userVehicleId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<VehicleServiceDefaultScheduleResponse>> GetDefaultScheduleAsync(
            Guid vehicleModelId,
            string partCategoryCode,
            CancellationToken cancellationToken = default);
    }
}
