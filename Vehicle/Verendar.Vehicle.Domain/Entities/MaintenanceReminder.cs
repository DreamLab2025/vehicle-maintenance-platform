using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceReminder : BaseEntity
    {
        [Required]
        public Guid VehiclePartTrackingId { get; set; }

        public int CurrentOdometer { get; set; }

        public int TargetOdometer { get; set; }

        public DateOnly? TargetDate { get; set; }

        public ReminderLevel Level { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentageRemaining { get; set; }

        public bool IsNotified { get; set; } = false;

        public DateOnly? NotifiedDate { get; set; }

        public bool IsDismissed { get; set; } = false;

        public DateOnly? DismissedDate { get; set; }

        public VehiclePartTracking PartTracking { get; set; } = null!;
    }
}
