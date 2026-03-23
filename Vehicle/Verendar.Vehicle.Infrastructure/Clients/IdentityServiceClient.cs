using Verendar.Common.Http;
using Verendar.Vehicle.Application.Clients;

namespace Verendar.Vehicle.Infrastructure.Clients
{
    public class IdentityServiceClient(HttpClient httpClient, ILogger<IdentityServiceClient> logger)
        : BaseServiceClient<IdentityServiceClient>(httpClient, logger), IIdentityServiceClient
    {
        protected override string ServiceName => "Identity Service";

        public async Task<string?> GetUserEmailByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling {ServiceName}: GET /api/internal/users/{UserId}/email", ServiceName, userId);

                var response = await _httpClient.GetAsync($"/api/internal/users/{userId}/email", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("{ServiceName} returned error: {StatusCode}", ServiceName, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = System.Text.Json.JsonSerializer.Deserialize<UserEmailResponse>(json, JsonOptions);
                return result?.Email;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling {ServiceName} for user {UserId}", ServiceName, userId);
                return null;
            }
        }

        private record UserEmailResponse(string Email);
    }
}
