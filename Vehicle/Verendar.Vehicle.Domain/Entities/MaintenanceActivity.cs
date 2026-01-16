using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    public class MaintenanceActivity : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }
        [ForeignKey(nameof(UserVehicleId))]
        public UserVehicle UserVehicle { get; set; } = null!;

        public DateTime PerformedDate { get; set; }
        public int OdometerAtTime { get; set; }

        [MaxLength(200)]
        public string? GarageName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public ICollection<MaintenanceActivityDetail> Details { get; set; } = new List<MaintenanceActivityDetail>();
    }
}
