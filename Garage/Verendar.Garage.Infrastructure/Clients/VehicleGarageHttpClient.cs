using System.Net.Http.Headers;
using System.Text.Json;
using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos;
using Verendar.Vehicle.Contracts.Dtos.Internal;

namespace Verendar.Garage.Infrastructure.Clients;

public class VehicleGarageHttpClient(
    HttpClient httpClient,
    IServiceTokenProvider serviceTokenProvider,
    ILogger<VehicleGarageHttpClient> logger) : IVehicleGarageClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<BookingVehicleSummary?> GetUserVehicleForBookingAsync(
        Guid ownerUserId,
        Guid userVehicleId,
        CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/internal/garage/user-vehicles/{userVehicleId}?ownerUserId={ownerUserId}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                serviceTokenProvider.GenerateServiceToken());

            var response = await httpClient.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Vehicle garage partner vehicle failed {Status} {Body}", response.StatusCode, json);
                return null;
            }

            var envelope = JsonSerializer.Deserialize<ApiResponse<GaragePartnerUserVehicleDto>>(json, JsonOptions);
            if (envelope?.Data is null)
                return null;

            var d = envelope.Data;
            return new BookingVehicleSummary
            {
                UserVehicleId = d.Id,
                LicensePlate = d.LicensePlate,
                Vin = d.Vin,
                CurrentOdometer = d.CurrentOdometer,
                ModelName = d.ModelName,
                BrandName = d.BrandName,
                VariantColor = d.VariantColor,
                ImageUrl = d.ImageUrl
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Vehicle garage partner vehicle error owner {OwnerId} vehicle {VehicleId}",
                ownerUserId, userVehicleId);
            return null;
        }
    }
}
