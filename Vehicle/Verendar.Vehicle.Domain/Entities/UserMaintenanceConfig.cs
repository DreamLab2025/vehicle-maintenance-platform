using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class UserMaintenanceConfig : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }
        [ForeignKey(nameof(UserVehicleId))]
        public UserVehicle UserVehicle { get; set; } = null!;

        [Required]
        public Guid VehiclePartId { get; set; }
        [ForeignKey(nameof(VehiclePartId))]
        public VehiclePart VehiclePart { get; set; } = null!;

        public int LastServiceOdometer { get; set; }
        public DateTime LastServiceDate { get; set; }

        public int? CustomDistanceInterval { get; set; }
        public int? CustomTimeIntervalMonth { get; set; }

        public DateTime? PredictedDueDate { get; set; }

        public bool IsIgnored { get; set; } = false;
    }
}
