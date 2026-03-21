namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(TrackingCycleId), nameof(Level))]
    public class MaintenanceReminder : BaseEntity
    {
        [Required]
        public Guid TrackingCycleId { get; set; }

        public int CurrentOdometer { get; set; }

        public int TargetOdometer { get; set; }

        public DateOnly? TargetDate { get; set; }

        public ReminderLevel Level { get; set; }

        public decimal PercentageRemaining { get; set; }

        public bool IsNotified { get; set; } = false;

        public DateOnly? NotifiedDate { get; set; }

        public bool IsDismissed { get; set; } = false;

        public DateOnly? DismissedDate { get; set; }

        public ReminderStatus Status { get; set; } = ReminderStatus.Active;

        public TrackingCycle TrackingCycle { get; set; } = null!;
    }
}
