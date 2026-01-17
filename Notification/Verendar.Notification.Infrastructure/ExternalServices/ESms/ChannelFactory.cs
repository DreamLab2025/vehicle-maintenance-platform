using System;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Infrastructure.ExternalServices.ESms;

public class ChannelFactory : IChannelFactory
{
    private readonly IEnumerable<INotificationChannel> _channels;
    public ChannelFactory(IEnumerable<INotificationChannel> channels) => _channels = channels;

    public INotificationChannel GetChannel(NotificationChannel type)
    {
        var channel = _channels.FirstOrDefault(x => x.ChannelType == type);
        return channel ?? throw new ArgumentException($"No service registered for channel {type}");
    }
}
