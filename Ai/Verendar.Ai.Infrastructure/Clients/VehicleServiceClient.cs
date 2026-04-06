using System.Net.Http.Json;
using Verendar.Common.Http;

namespace Verendar.Ai.Infrastructure.Clients
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
            string partCategorySlug,
            CancellationToken cancellationToken = default) =>
            GetAsync<VehicleServiceDefaultScheduleResponse>(
                $"/api/internal/vehicles/models/{vehicleModelId}/part-categories/{partCategorySlug}/default-schedule",
                $"default schedule for model {vehicleModelId}, part {partCategorySlug}",
                cancellationToken);

        public Task<ApiResponse<VehicleServiceOdometerSummaryResponse>> GetOdometerSummaryAsync(
            Guid userVehicleId,
            CancellationToken cancellationToken = default) =>
            GetAsync<VehicleServiceOdometerSummaryResponse>(
                $"/api/internal/vehicles/user-vehicles/{userVehicleId}/odometer-summary",
                $"odometer summary for user vehicle {userVehicleId}",
                cancellationToken);

        public Task<ApiResponse<List<VehicleServiceBaselinePartItem>>> GetBaselinePartsAsync(
            Guid userVehicleId,
            CancellationToken cancellationToken = default) =>
            GetAsync<List<VehicleServiceBaselinePartItem>>(
                $"/api/internal/vehicles/user-vehicles/{userVehicleId}/baseline-parts",
                $"baseline parts for user vehicle {userVehicleId}",
                cancellationToken);

        public async Task<ApiResponse<object>> ApplyTrackingInternalAsync(
            Guid vehicleId,
            Guid userId,
            VehicleServiceApplyTrackingRequest request,
            CancellationToken cancellationToken = default)
        {
            var url = $"/api/internal/vehicles/user-vehicles/{vehicleId}/apply-tracking?userId={userId}";
            try
            {
                _logger.LogInformation("Calling {ServiceName}: POST {Url}", ServiceName, url);
                var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("{ServiceName} ApplyTracking returned error: {StatusCode} - {Content}", ServiceName, response.StatusCode, error);
                    return ApiResponse<object>.FailureResponse($"{ServiceName} error: {response.StatusCode}");
                }
                return ApiResponse<object>.SuccessResponse(new object());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling {ServiceName} ApplyTracking for vehicle {VehicleId}", ServiceName, vehicleId);
                return ApiResponse<object>.FailureResponse($"Error calling {ServiceName}: {ex.Message}");
            }
        }
    }
}
