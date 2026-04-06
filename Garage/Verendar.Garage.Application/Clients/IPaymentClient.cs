using Verendar.Garage.Domain.ValueObjects;

using Verendar.Garage.Application.Dtos.Clients;
namespace Verendar.Garage.Application.Clients;

public interface IPaymentClient
{
    Task<PaymentInitiateResult> InitiateAsync(
        Guid bookingId,
        Money amount,
        string returnUrl,
        CancellationToken ct = default);

    Task<bool> RefundAsync(
        Guid paymentId,
        Money amount,
        CancellationToken ct = default);
}


