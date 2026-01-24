namespace Verendar.Common.Events
{
    public interface IEvent
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
        string EventType { get; }
    }
}
