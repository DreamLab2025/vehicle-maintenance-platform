using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    public class StandardMaintenanceSchedule
    {
        [Required]
        public Guid VehicleModelId { get; set; }
        [ForeignKey(nameof(VehicleModelId))]
        public VehicleModel VehicleModel { get; set; } = null!;

        [Required]
        public Guid ConsumableItemId { get; set; }
        [ForeignKey(nameof(ConsumableItemId))]
        public ConsumableItem ConsumableItem { get; set; } = null!;

        public int? InitialDistance { get; set; } // Rodai (km)
        public int DistanceInterval { get; set; } // Chu kỳ lặp (km)
        public int TimeIntervalMonth { get; set; } // Chu kỳ lặp (tháng)

        public EntityStatus Status { get; set; } = EntityStatus.Active;
    }
}
