using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Verendar.Ai.Application.Dtos.VehicleService;
using Verendar.Common.Shared;

namespace Verendar.Ai.Application.Clients
{
    public class VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger) : IVehicleServiceClient
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<VehicleServiceClient> _logger = logger;

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public Task<ApiResponse<VehicleServiceUserVehicleResponse>> GetUserVehicleByIdAsync(
            Guid userVehicleId,
            CancellationToken cancellationToken = default) =>
            GetAsync<VehicleServiceUserVehicleResponse>(
                $"/api/v1/user-vehicles/{userVehicleId}",
                $"user vehicle {userVehicleId}",
                cancellationToken);

        public Task<ApiResponse<VehicleServiceDefaultScheduleResponse>> GetDefaultScheduleAsync(
            Guid vehicleModelId,
            string partCategoryCode,
            CancellationToken cancellationToken = default) =>
            GetAsync<VehicleServiceDefaultScheduleResponse>(
                $"/api/v1/vehicle-models/{vehicleModelId}/part-categories/{partCategoryCode}/default-schedule",
                $"default schedule for model {vehicleModelId}, part {partCategoryCode}",
                cancellationToken);

        private async Task<ApiResponse<T>> GetAsync<T>(
            string url,
            string logContext,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Calling Vehicle Service: GET {Url}", url);

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Vehicle Service returned error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    return ApiResponse<T>.FailureResponse($"Vehicle Service error: {response.StatusCode}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, JsonOptions);

                if (apiResponse == null || !apiResponse.IsSuccess || apiResponse.Data == null)
                {
                    _logger.LogWarning("Vehicle Service returned unsuccessful response: {Message}", apiResponse?.Message);
                    return ApiResponse<T>.FailureResponse(apiResponse?.Message ?? "Failed to get data from Vehicle Service");
                }

                _logger.LogInformation("Successfully retrieved {LogContext}", logContext);
                return apiResponse;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Circuit breaker is open. Vehicle Service is unavailable");
                return ApiResponse<T>.FailureResponse("Vehicle Service is temporarily unavailable. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Vehicle Service: {LogContext}", logContext);
                return ApiResponse<T>.FailureResponse($"Error calling Vehicle Service: {ex.Message}");
            }
        }
    }
}
