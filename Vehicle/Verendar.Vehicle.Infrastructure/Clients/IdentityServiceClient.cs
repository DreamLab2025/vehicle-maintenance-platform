using System.Net.Http.Headers;
using System.Text.Json;
using Verendar.Common.Http;
using Verendar.Common.Jwt;
using Verendar.Vehicle.Application.Clients;

namespace Verendar.Vehicle.Infrastructure.Clients
{
    public class IdentityServiceClient(
        HttpClient httpClient,
        IServiceTokenProvider serviceTokenProvider,
        ILogger<IdentityServiceClient> logger)
        : BaseServiceClient<IdentityServiceClient>(httpClient, logger), IIdentityServiceClient
    {
        private readonly IServiceTokenProvider _serviceTokenProvider = serviceTokenProvider;

        protected override string ServiceName => "Identity Service";

        public async Task<string?> GetUserEmailByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling {ServiceName}: GET /api/internal/users/{UserId}/email", ServiceName, userId);

                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/internal/users/{userId}/email");
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    _serviceTokenProvider.GenerateServiceToken());

                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("{ServiceName} returned error: {StatusCode}", ServiceName, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return TryReadEmailFromApiEnvelope(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling {ServiceName} for user {UserId}", ServiceName, userId);
                return null;
            }
        }

        private static string? TryReadEmailFromApiEnvelope(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.ValueKind == JsonValueKind.Object &&
                    data.TryGetProperty("email", out var emailEl) &&
                    emailEl.ValueKind == JsonValueKind.String)
                {
                    var s = emailEl.GetString();
                    return string.IsNullOrWhiteSpace(s) ? null : s;
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}
