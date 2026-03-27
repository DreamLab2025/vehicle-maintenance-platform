using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Notification.Application.Clients;
using Verendar.Vehicle.Contracts.Dtos.Internal;

namespace Verendar.Notification.Infrastructure.Clients;

public class VehicleMaintenanceReminderLookupClient(
    HttpClient httpClient,
    IServiceTokenProvider serviceTokenProvider,
    ILogger<VehicleMaintenanceReminderLookupClient> logger) : IVehicleMaintenanceReminderLookupClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IServiceTokenProvider _serviceTokenProvider = serviceTokenProvider;
    private readonly ILogger<VehicleMaintenanceReminderLookupClient> _logger = logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<MaintenanceReminderLookupItemResponse>?> LookupAsync(
        Guid userId,
        IReadOnlyList<Guid> reminderIds,
        CancellationToken cancellationToken = default)
    {
        if (reminderIds.Count == 0)
            return [];

        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "api/internal/maintenance-reminders/lookup")
            {
                Content = JsonContent.Create(new MaintenanceReminderLookupRequest
                {
                    UserId = userId,
                    ReminderIds = reminderIds.ToList()
                })
            };

            message.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _serviceTokenProvider.GenerateServiceToken());

            var response = await _httpClient.SendAsync(message, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Vehicle maintenance reminder lookup failed: {Status} {Body}",
                    response.StatusCode, content);
                return null;
            }

            var envelope = JsonSerializer.Deserialize<ApiResponse<IReadOnlyList<MaintenanceReminderLookupItemResponse>>>(
                content,
                JsonOptions);

            if (envelope?.IsSuccess != true || envelope.Data == null)
                return null;

            return envelope.Data.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vehicle maintenance reminder lookup error");
            return null;
        }
    }
}
