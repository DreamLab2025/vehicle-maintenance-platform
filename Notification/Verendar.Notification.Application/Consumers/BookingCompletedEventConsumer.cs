using MassTransit;
using Verendar.Garage.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class BookingCompletedEventConsumer(ILogger<BookingCompletedEventConsumer> logger) : IConsumer<BookingCompletedEvent>
{
    public Task Consume(ConsumeContext<BookingCompletedEvent> context)
    {
        var m = context.Message;
        logger.LogInformation(
            "BookingCompleted: BookingId={BookingId} UserVehicle={UserVehicleId} Odo={Odo}",
            m.BookingId, m.UserVehicleId, m.CurrentOdometer);
        return Task.CompletedTask;
    }
}
