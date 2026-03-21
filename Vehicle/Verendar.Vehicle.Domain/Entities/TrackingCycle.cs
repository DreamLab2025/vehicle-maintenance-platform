namespace Verendar.Vehicle.Domain.Entities
{
    public class TrackingCycle : BaseEntity
    {
        [Required]
        public Guid PartTrackingId { get; set; }

        public int StartOdometer { get; set; }

        public DateOnly StartDate { get; set; }

        public int? TargetOdometer { get; set; }

        public DateOnly? TargetDate { get; set; }

        public CycleStatus Status { get; set; } = CycleStatus.Active;

        public PartTracking PartTracking { get; set; } = null!;

        public List<MaintenanceReminder> Reminders { get; set; } = [];
    }
}
