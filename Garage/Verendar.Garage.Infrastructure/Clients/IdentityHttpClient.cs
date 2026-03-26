using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Verendar.Common.Jwt;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos.Clients;

namespace Verendar.Garage.Infrastructure.Clients;

public class IdentityHttpClient(
    HttpClient httpClient,
    IServiceTokenProvider serviceTokenProvider,
    ILogger<IdentityHttpClient> logger) : IIdentityClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IServiceTokenProvider _serviceTokenProvider = serviceTokenProvider;
    private readonly ILogger<IdentityHttpClient> _logger = logger;

    public Task<Guid?> CreateMechanicUserAsync(CreateMemberUserRequest request, CancellationToken ct = default) =>
        CreateMemberAsync("/api/internal/users/mechanic", request, ct);

    public Task<Guid?> CreateManagerUserAsync(CreateMemberUserRequest request, CancellationToken ct = default) =>
        CreateMemberAsync("/api/internal/users/manager", request, ct);

    private async Task<Guid?> CreateMemberAsync(string path, CreateMemberUserRequest request, CancellationToken ct)
    {
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(request)
            };

            message.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _serviceTokenProvider.GenerateServiceToken());

            var response = await _httpClient.SendAsync(message, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Identity create member failed: {Path} {Status} {Body}",
                    path, response.StatusCode, content);
                return null;
            }

            if (TryExtractUserId(content, out var userId))
                return userId;

            _logger.LogWarning("Identity create member success but cannot parse userId: {Path}", path);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling identity endpoint {Path}", path);
            return null;
        }
    }

    private static bool TryExtractUserId(string json, out Guid userId)
    {
        userId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // direct payload: { "userId": "..." }
        if (TryReadUserId(root, out userId))
            return true;

        // wrapped payload: { "data": { "userId": "..." } }
        if (root.TryGetProperty("data", out var dataElement) && TryReadUserId(dataElement, out userId))
            return true;

        return false;
    }

    private static bool TryReadUserId(JsonElement element, out Guid userId)
    {
        userId = Guid.Empty;
        if (!element.TryGetProperty("userId", out var userIdElement))
            return false;

        if (userIdElement.ValueKind == JsonValueKind.String
            && Guid.TryParse(userIdElement.GetString(), out userId))
            return true;

        return userIdElement.ValueKind == JsonValueKind.String
            ? false
            : Guid.TryParse(userIdElement.ToString(), out userId);
    }
}
