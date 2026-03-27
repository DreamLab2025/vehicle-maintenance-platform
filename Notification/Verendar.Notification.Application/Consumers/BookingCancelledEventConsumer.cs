using MassTransit;
using Verendar.Garage.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class BookingCancelledEventConsumer(ILogger<BookingCancelledEventConsumer> logger) : IConsumer<BookingCancelledEvent>
{
    public Task Consume(ConsumeContext<BookingCancelledEvent> context)
    {
        var m = context.Message;
        logger.LogInformation(
            "BookingCancelled: BookingId={BookingId} Customer={Customer}",
            m.BookingId, m.CustomerUserId);
        return Task.CompletedTask;
    }
}
