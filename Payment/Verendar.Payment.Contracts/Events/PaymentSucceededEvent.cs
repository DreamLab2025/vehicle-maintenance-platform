using Verendar.Common.Contracts;

namespace Verendar.Payment.Contracts.Events;

public class PaymentSucceededEvent : BaseEvent
{
    public override string EventType => "payment.succeeded.v1";

    public Guid PaymentId { get; set; }

    /// <summary>The business entity this payment is for (e.g. BookingId).</summary>
    public Guid ReferenceId { get; set; }

    /// <summary>The type of reference entity (e.g. "Booking").</summary>
    public string ReferenceType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";
}
