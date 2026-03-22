using Verendar.Common.Http;

namespace Verendar.Ai.Application.Clients
{
    public class VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
        : BaseServiceClient<VehicleServiceClient>(httpClient, logger), IVehicleServiceClient
    {
        protected override string ServiceName => "Vehicle Service";

        public Task<ApiResponse<VehicleServiceUserVehicleResponse>> GetUserVehicleByIdAsync(
            Guid userVehicleId,
            CancellationToken cancellationToken = default) =>
            GetAsync<VehicleServiceUserVehicleResponse>(
                $"/api/internal/vehicles/user-vehicles/{userVehicleId}",
                $"user vehicle {userVehicleId}",
                cancellationToken);

        public Task<ApiResponse<VehicleServiceDefaultScheduleResponse>> GetDefaultScheduleAsync(
            Guid vehicleModelId,
            string partCategoryCode,
            CancellationToken cancellationToken = default) =>
            GetAsync<VehicleServiceDefaultScheduleResponse>(
                $"/api/internal/vehicles/models/{vehicleModelId}/part-categories/{partCategoryCode}/default-schedule",
                $"default schedule for model {vehicleModelId}, part {partCategoryCode}",
                cancellationToken);
    }
}
