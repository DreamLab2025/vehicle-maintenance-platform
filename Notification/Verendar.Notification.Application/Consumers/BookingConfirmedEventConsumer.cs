using MassTransit;
using Verendar.Garage.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class BookingConfirmedEventConsumer(ILogger<BookingConfirmedEventConsumer> logger) : IConsumer<BookingConfirmedEvent>
{
    public Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var m = context.Message;
        logger.LogInformation(
            "BookingConfirmed: BookingId={BookingId} Customer={Customer} MechanicMember={MechanicMemberId}",
            m.BookingId, m.CustomerUserId, m.MechanicMemberId);
        return Task.CompletedTask;
    }
}
