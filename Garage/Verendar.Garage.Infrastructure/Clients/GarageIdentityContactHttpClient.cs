using System.Net.Http.Headers;
using System.Text.Json;
using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Infrastructure.Clients;

public class GarageIdentityContactHttpClient(
    HttpClient httpClient,
    IServiceTokenProvider serviceTokenProvider,
    ILogger<GarageIdentityContactHttpClient> logger) : IGarageIdentityContactClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<BookingCustomerSummary?> GetCustomerContactAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/internal/users/{userId}/garage-contact");
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                serviceTokenProvider.GenerateServiceToken());

            var response = await httpClient.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Identity garage-contact failed {Status} {Body}", response.StatusCode, json);
                return null;
            }

            var envelope = JsonSerializer.Deserialize<ApiResponse<ContactPayload>>(json, JsonOptions);
            if (envelope?.Data is null)
                return null;

            return new BookingCustomerSummary
            {
                FullName = envelope.Data.FullName,
                Email = envelope.Data.Email,
                PhoneNumber = envelope.Data.PhoneNumber
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Identity garage-contact error user {UserId}", userId);
            return null;
        }
    }

    private sealed class ContactPayload
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
