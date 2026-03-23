using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Payment.Contracts.Events;

namespace Verendar.Garage.Application.Consumers;

/// <summary>
/// Handles PaymentRefundedEvent — full logic implemented in Task 8.
/// Confirms booking cancellation and publishes BookingCancelledEvent.
/// </summary>
public class PaymentRefundedEventConsumer(ILogger<PaymentRefundedEventConsumer> logger)
    : IConsumer<PaymentRefundedEvent>
{
    private readonly ILogger<PaymentRefundedEventConsumer> _logger = logger;

    public Task Consume(ConsumeContext<PaymentRefundedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "PaymentRefundedEvent received — PaymentId: {PaymentId}, ReferenceId: {ReferenceId}, ReferenceType: {ReferenceType}",
            message.PaymentId, message.ReferenceId, message.ReferenceType);

        // TODO Task 8: confirm booking cancellation, publish BookingCancelledEvent
        return Task.CompletedTask;
    }
}
