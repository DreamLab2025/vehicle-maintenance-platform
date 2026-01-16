using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verender.Common.Databases.Base;

namespace Verender.Vehicle.Domain.Entities
{
    public class OdometerHistory : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }
        [ForeignKey(nameof(UserVehicleId))]
        public UserVehicle UserVehicle { get; set; } = null!;

        public int OdometerValue { get; set; }
        public DateTime RecordedAt { get; set; }
        public MaintenanceSource Source { get; set; }
    }

    public enum MaintenanceSource
    {
        ManualInput = 1,
    }
}
