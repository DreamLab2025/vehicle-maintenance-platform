using System.ComponentModel.DataAnnotations;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class VehiclePartTracking : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }

        [Required]
        public Guid PartCategoryId { get; set; }

        public Guid? CurrentPartProductId { get; set; }

        [MaxLength(50)]
        public string? InstanceIdentifier { get; set; }

        public int? LastReplacementOdometer { get; set; }

        public DateOnly? LastReplacementDate { get; set; }

        public int? CustomKmInterval { get; set; }

        public int? CustomMonthsInterval { get; set; }

        public int? PredictedNextOdometer { get; set; }

        public DateOnly? PredictedNextDate { get; set; }

        public bool IsIgnored { get; set; } = false;

        [MaxLength(2000)]
        public string? UserConditionDescription { get; set; }

        [MaxLength(2000)]
        public string? AiAnalysisResult { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public UserVehicle UserVehicle { get; set; } = null!;

        public PartCategory PartCategory { get; set; } = null!;

        public PartProduct? CurrentPartProduct { get; set; }

        public List<MaintenanceReminder> Reminders { get; set; } = [];
    }
}
