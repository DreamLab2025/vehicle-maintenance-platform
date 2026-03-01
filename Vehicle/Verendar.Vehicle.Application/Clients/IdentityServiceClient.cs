using System.Text.Json;

namespace Verendar.Vehicle.Application.Clients
{
    public class IdentityServiceClient(HttpClient httpClient) : IIdentityServiceClient
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<string?> GetUserEmailByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"/api/internal/users/{userId}/email", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<UserEmailResponse>(json, options);
            return result?.Email;
        }

        private record UserEmailResponse(string Email);
    }
}
