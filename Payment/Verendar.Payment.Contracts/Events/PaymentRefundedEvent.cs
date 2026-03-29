using Verendar.Common.Contracts;

namespace Verendar.Payment.Contracts.Events;

public class PaymentRefundedEvent : BaseEvent
{
    public override string EventType => "payment.refunded.v1";

    public Guid PaymentId { get; set; }

    public Guid ReferenceId { get; set; }

    public string ReferenceType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";
}
