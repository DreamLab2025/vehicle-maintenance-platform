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

    public Task<(Guid? UserId, string? ActualPassword)> CreateMechanicUserAsync(CreateMemberUserRequest request, CancellationToken ct = default) =>
        CreateMemberAsync("/api/internal/users/mechanic", request, ct);

    public Task<(Guid? UserId, string? ActualPassword)> CreateManagerUserAsync(CreateMemberUserRequest request, CancellationToken ct = default) =>
        CreateMemberAsync("/api/internal/users/manager", request, ct);

    public Task<bool> AssignRoleAsync(Guid userId, string role, CancellationToken ct = default) =>
        SendRoleRequestAsync(HttpMethod.Post, $"/api/internal/users/{userId}/roles", new { Role = role }, ct);

    public Task<bool> RevokeRoleAsync(Guid userId, string role, CancellationToken ct = default) =>
        SendRoleRequestAsync(HttpMethod.Delete, $"/api/internal/users/{userId}/roles/{role}", null, ct);

    public async Task<bool> BulkDeactivateAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        if (ids.Count == 0) return true;
        return await SendRoleRequestAsync(HttpMethod.Post, "/api/internal/users/bulk-deactivate", new { UserIds = ids }, ct);
    }

    private async Task<bool> SendRoleRequestAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        try
        {
            using var message = new HttpRequestMessage(method, path);
            if (body != null)
                message.Content = JsonContent.Create(body);

            message.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _serviceTokenProvider.GenerateServiceToken());

            var response = await _httpClient.SendAsync(message, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Identity role request failed: {Method} {Path} {Status} {Body}",
                    method, path, response.StatusCode, content);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling identity role endpoint {Method} {Path}", method, path);
            return false;
        }
    }

    private async Task<(Guid? UserId, string? ActualPassword)> CreateMemberAsync(string path, CreateMemberUserRequest request, CancellationToken ct)
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
                return (null, null);
            }

            if (TryExtractCreateMemberResult(content, out var userId, out var actualPassword))
                return (userId, actualPassword);

            _logger.LogWarning("Identity create member success but cannot parse response: {Path} Body={Body}", path, content);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling identity endpoint {Path}", path);
            return (null, null);
        }
    }

    private static bool TryExtractCreateMemberResult(string json, out Guid userId, out string? actualPassword)
    {
        userId = Guid.Empty;
        actualPassword = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // direct payload: { "userId": "...", "actualPassword": "..." }
        if (TryReadCreateMemberResult(root, out userId, out actualPassword))
            return true;

        // wrapped payload: { "data": { "userId": "...", "actualPassword": "..." } }
        if (root.TryGetProperty("data", out var dataElement) && TryReadCreateMemberResult(dataElement, out userId, out actualPassword))
            return true;

        return false;
    }

    private static bool TryReadCreateMemberResult(JsonElement element, out Guid userId, out string? actualPassword)
    {
        userId = Guid.Empty;
        actualPassword = null;

        if (!element.TryGetProperty("userId", out var userIdElement))
            return false;

        if (userIdElement.ValueKind != JsonValueKind.String
            || !Guid.TryParse(userIdElement.GetString(), out userId))
            return false;

        if (element.TryGetProperty("actualPassword", out var pwElement)
            && pwElement.ValueKind == JsonValueKind.String)
            actualPassword = pwElement.GetString();

        return true;
    }
}
