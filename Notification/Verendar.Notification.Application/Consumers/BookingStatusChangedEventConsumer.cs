using MassTransit;
using Verendar.Garage.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class BookingStatusChangedEventConsumer(ILogger<BookingStatusChangedEventConsumer> logger)
    : IConsumer<BookingStatusChangedEvent>
{
    public Task Consume(ConsumeContext<BookingStatusChangedEvent> context)
    {
        var m = context.Message;
        logger.LogInformation(
            "BookingStatusChanged: BookingId={BookingId} {From} → {To}",
            m.BookingId, m.FromStatus, m.ToStatus);
        return Task.CompletedTask;
    }
}
