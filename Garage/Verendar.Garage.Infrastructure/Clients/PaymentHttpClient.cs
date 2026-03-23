using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Infrastructure.Clients;

public class PaymentHttpClient(HttpClient httpClient, ILogger<PaymentHttpClient> logger) : IPaymentClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<PaymentHttpClient> _logger = logger;

    public async Task<PaymentInitiateResult> InitiateAsync(
        Guid bookingId,
        Money amount,
        string returnUrl,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/internal/payments/initiate",
                new { BookingId = bookingId, Amount = amount.Amount, Currency = amount.Currency, ReturnUrl = returnUrl },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Payment initiation failed for booking {BookingId}: {Error}", bookingId, error);
                return new PaymentInitiateResult(false, null, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<PaymentInitiateResponse>(cancellationToken: ct);
            return new PaymentInitiateResult(true, result!.PaymentId, result.PaymentUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for booking {BookingId}", bookingId);
            return new PaymentInitiateResult(false, null, null, ex.Message);
        }
    }

    public async Task<bool> RefundAsync(
        Guid paymentId,
        Money amount,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/internal/payments/refund",
                new { PaymentId = paymentId, Amount = amount.Amount, Currency = amount.Currency },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Refund failed for payment {PaymentId}: {Error}", paymentId, error);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", paymentId);
            return false;
        }
    }

    private sealed record PaymentInitiateResponse(Guid PaymentId, string PaymentUrl);
}
