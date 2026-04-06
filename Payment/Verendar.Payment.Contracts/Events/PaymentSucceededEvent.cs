using Verendar.Common.Contracts;

namespace Verendar.Payment.Contracts.Events;

public class PaymentSucceededEvent : BaseEvent
{
    public override string EventType => "payment.succeeded.v1";

    public Guid PaymentId { get; set; }

    public Guid ReferenceId { get; set; }

    public string ReferenceType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";
}
