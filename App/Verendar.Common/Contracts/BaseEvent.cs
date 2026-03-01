using Verendar.Common.Events;

namespace Verendar.Common.Contracts
{
    public abstract class BaseEvent : IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public abstract string EventType { get; }
    }
}
