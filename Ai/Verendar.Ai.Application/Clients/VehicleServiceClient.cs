using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Verendar.Ai.Application.Dtos.VehicleService;
using Verendar.Common.Shared;

namespace Verendar.Ai.Application.Clients;

public class VehicleServiceClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<VehicleServiceClient> logger) : IVehicleServiceClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<VehicleServiceClient> _logger = logger;

    public async Task<ApiResponse<VehicleServiceUserVehicleResponse>> GetUserVehicleByIdAsync(
        Guid userVehicleId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/user-vehicles/{userVehicleId}");
            
            // Forward JWT token from current request
            await AddAuthorizationHeaderAsync(request);

            _logger.LogInformation("Calling Vehicle Service: GET /api/v1/user-vehicles/{UserVehicleId}", userVehicleId);

            // Polly policies are automatically applied via AddPolicyHandler in Bootstrapping
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Vehicle Service returned error: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);

                return ApiResponse<VehicleServiceUserVehicleResponse>.FailureResponse(
                    $"Vehicle Service error: {response.StatusCode}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            // Using lightweight DTO - JSON serializer will automatically ignore extra fields
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<VehicleServiceUserVehicleResponse>>(
                jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiResponse == null || !apiResponse.IsSuccess || apiResponse.Data == null)
            {
                _logger.LogWarning("Vehicle Service returned unsuccessful response: {Message}", apiResponse?.Message);
                return ApiResponse<VehicleServiceUserVehicleResponse>.FailureResponse(
                    apiResponse?.Message ?? "Failed to get user vehicle");
            }

            _logger.LogInformation("Successfully retrieved user vehicle {UserVehicleId}", userVehicleId);
            return apiResponse;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker is open. Vehicle Service is unavailable");
            return ApiResponse<VehicleServiceUserVehicleResponse>.FailureResponse(
                "Vehicle Service is temporarily unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Vehicle Service to get user vehicle {UserVehicleId}", userVehicleId);
            return ApiResponse<VehicleServiceUserVehicleResponse>.FailureResponse(
                $"Error calling Vehicle Service: {ex.Message}");
        }
    }

    public async Task<ApiResponse<VehicleServiceDefaultScheduleResponse>> GetDefaultScheduleAsync(
        Guid vehicleModelId,
        string partCategoryCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v1/vehicle-models/{vehicleModelId}/part-categories/{partCategoryCode}/default-schedule");

            // Forward JWT token from current request
            await AddAuthorizationHeaderAsync(request);

            _logger.LogInformation(
                "Calling Vehicle Service: GET /api/v1/vehicle-models/{VehicleModelId}/part-categories/{PartCategoryCode}/default-schedule",
                vehicleModelId, partCategoryCode);

            // Polly policies are automatically applied via AddPolicyHandler in Bootstrapping
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Vehicle Service returned error: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);

                return ApiResponse<VehicleServiceDefaultScheduleResponse>.FailureResponse(
                    $"Vehicle Service error: {response.StatusCode}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<VehicleServiceDefaultScheduleResponse>>(
                jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiResponse == null || !apiResponse.IsSuccess || apiResponse.Data == null)
            {
                _logger.LogWarning(
                    "Vehicle Service returned unsuccessful response for schedule: {Message}",
                    apiResponse?.Message);
                return ApiResponse<VehicleServiceDefaultScheduleResponse>.FailureResponse(
                    apiResponse?.Message ?? "Failed to get default schedule");
            }

            _logger.LogInformation(
                "Successfully retrieved default schedule for model {VehicleModelId}, part {PartCategoryCode}",
                vehicleModelId, partCategoryCode);
            return apiResponse;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker is open. Vehicle Service is unavailable");
            return ApiResponse<VehicleServiceDefaultScheduleResponse>.FailureResponse(
                "Vehicle Service is temporarily unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error calling Vehicle Service to get default schedule for model {VehicleModelId}, part {PartCategoryCode}",
                vehicleModelId, partCategoryCode);
            return ApiResponse<VehicleServiceDefaultScheduleResponse>.FailureResponse(
                $"Error calling Vehicle Service: {ex.Message}");
        }
    }

    private async Task AddAuthorizationHeaderAsync(HttpRequestMessage request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.ContainsKey("Authorization") == true)
        {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);
                _logger.LogDebug("Forwarded authorization header to Vehicle Service");
            }
        }
        else
        {
            _logger.LogWarning("No authorization header found in current request context");
        }
    }
}
