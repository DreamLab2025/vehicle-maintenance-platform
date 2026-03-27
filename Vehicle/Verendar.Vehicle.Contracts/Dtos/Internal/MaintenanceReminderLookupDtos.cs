namespace Verendar.Vehicle.Contracts.Dtos.Internal;

public class MaintenanceReminderLookupRequest
{
    public Guid UserId { get; set; }

    public List<Guid> ReminderIds { get; set; } = [];
}

public class MaintenanceReminderLookupItemResponse
{
    public Guid ReminderId { get; set; }

    public Guid PartTrackingId { get; set; }

    public string ReminderStatus { get; set; } = string.Empty;

    public DateOnly? LastReplacementDate { get; set; }

    public int? LastReplacementOdometer { get; set; }
}
