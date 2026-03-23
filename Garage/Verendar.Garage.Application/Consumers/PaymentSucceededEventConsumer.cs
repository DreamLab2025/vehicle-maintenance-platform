using MassTransit;
using Microsoft.Extensions.Logging;
using Verendar.Payment.Contracts.Events;

namespace Verendar.Garage.Application.Consumers;

/// <summary>
/// Handles PaymentSucceededEvent — full logic implemented in Task 8.
/// Transitions the booking from Pending to AwaitingConfirmation.
/// </summary>
public class PaymentSucceededEventConsumer(ILogger<PaymentSucceededEventConsumer> logger)
    : IConsumer<PaymentSucceededEvent>
{
    private readonly ILogger<PaymentSucceededEventConsumer> _logger = logger;

    public Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "PaymentSucceededEvent received — PaymentId: {PaymentId}, ReferenceId: {ReferenceId}, ReferenceType: {ReferenceType}",
            message.PaymentId, message.ReferenceId, message.ReferenceType);

        // TODO Task 8: update Booking.Status = AwaitingConfirmation, write BookingStatusHistory, publish BookingConfirmedEvent
        return Task.CompletedTask;
    }
}
