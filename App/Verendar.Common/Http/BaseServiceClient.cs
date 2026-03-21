using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Verendar.Common.Shared;

namespace Verendar.Common.Http
{
    public abstract class BaseServiceClient<TClient>(HttpClient httpClient, ILogger<TClient> logger)
        : IBaseClient where TClient : class
    {
        protected readonly HttpClient _httpClient = httpClient;
        protected readonly ILogger<TClient> _logger = logger;
        protected static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        protected virtual string ServiceName => typeof(TClient).Name.Replace("Client", string.Empty);

        protected async Task<ApiResponse<T>> GetAsync<T>(
            string url,
            string logContext,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling {ServiceName}: GET {Url}", ServiceName, url);

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("{ServiceName} returned error: {StatusCode} - {Content}",
                        ServiceName, response.StatusCode, errorContent);
                    return ApiResponse<T>.FailureResponse($"{ServiceName} error: {response.StatusCode}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, JsonOptions);

                if (apiResponse == null || !apiResponse.IsSuccess || apiResponse.Data == null)
                {
                    _logger.LogWarning("{ServiceName} returned unsuccessful response: {Message}",
                        ServiceName, apiResponse?.Message);
                    return ApiResponse<T>.FailureResponse(apiResponse?.Message ?? $"Failed to get data from {ServiceName}");
                }

                _logger.LogInformation("Successfully retrieved {LogContext}", logContext);
                return apiResponse;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Circuit breaker is open. {ServiceName} is unavailable", ServiceName);
                return ApiResponse<T>.FailureResponse($"{ServiceName} is temporarily unavailable. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling {ServiceName}: {LogContext}", ServiceName, logContext);
                return ApiResponse<T>.FailureResponse($"Error calling {ServiceName}: {ex.Message}");
            }
        }
    }
}
