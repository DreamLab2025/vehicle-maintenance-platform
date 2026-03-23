using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos.Clients;

namespace Verendar.Garage.Infrastructure.Clients;

public class VietQRHttpClient(HttpClient httpClient, ILogger<VietQRHttpClient> logger) : IVietQRClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<VietQRHttpClient> _logger = logger;

    public async Task<BusinessInfoDto?> LookupBusinessAsync(string taxCode, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v2/business/{taxCode}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("VietQR business lookup returned {StatusCode} for MST {TaxCode}",
                    (int)response.StatusCode, taxCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<VietQRResponse>(cancellationToken: ct);

            if (result is null || result.Code != "00" || result.Data is null)
            {
                _logger.LogWarning("VietQR lookup: MST {TaxCode} not found or inactive. Code={Code}",
                    taxCode, result?.Code);
                return null;
            }

            return new BusinessInfoDto(
                TaxCode: result.Data.Id,
                Name: result.Data.Name,
                InternationalName: result.Data.InternationalName,
                ShortName: result.Data.ShortName,
                Address: result.Data.Address,
                Status: result.Data.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling VietQR business lookup for MST {TaxCode}", taxCode);
            return null;
        }
    }

    private sealed record VietQRResponse(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("desc")] string Desc,
        [property: JsonPropertyName("data")] VietQRBusinessData? Data);

    private sealed record VietQRBusinessData(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("internationalName")] string? InternationalName,
        [property: JsonPropertyName("shortName")] string? ShortName,
        [property: JsonPropertyName("address")] string? Address,
        [property: JsonPropertyName("status")] string? Status);
}
