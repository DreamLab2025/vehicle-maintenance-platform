namespace Verendar.Garage.Application.Dtos.Clients;
public record PaymentInitiateResult(
    bool Success,
    Guid? PaymentId,
    string? PaymentUrl,
    string? ErrorMessage);