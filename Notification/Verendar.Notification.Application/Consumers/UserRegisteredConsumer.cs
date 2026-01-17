using System;
using MassTransit;
using Verender.Identity.Contracts.Events;

namespace Verendar.Notification.Application.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        throw new NotImplementedException();
    }
}
