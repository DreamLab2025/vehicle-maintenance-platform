using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Application.Clients;

/// <summary>HTTP client abstraction for the Payment service (cross-service calls).</summary>
public interface IPaymentClient
{
    /// <summary>Creates a payment for a booking and returns the provider checkout URL.</summary>
    Task<PaymentInitiateResult> InitiateAsync(
        Guid bookingId,
        Money amount,
        string returnUrl,
        CancellationToken ct = default);

    /// <summary>Requests a refund for an existing payment.</summary>
    Task<bool> RefundAsync(
        Guid paymentId,
        Money amount,
        CancellationToken ct = default);
}

public record PaymentInitiateResult(
    bool Success,
    Guid? PaymentId,
    string? PaymentUrl,
    string? ErrorMessage);
