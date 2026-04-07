using Verendar.Common.Contracts;

namespace Verendar.Garage.Contracts.Events;

public class BookingConfirmedEvent : BaseEvent
{
    public override string EventType => "garage.booking.confirmed.v1";

    public Guid BookingId { get; set; }
    public Guid CustomerUserId { get; set; }
    public Guid UserVehicleId { get; set; }
    public Guid GarageId { get; set; }
    public Guid GarageBranchId { get; set; }
    public Guid MechanicMemberId { get; set; }
    public Guid MechanicUserId { get; set; }
    public string MechanicDisplayName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string ItemsSummary { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
}
