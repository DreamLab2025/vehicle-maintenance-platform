using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Domain.Entities
{
    /// <summary>
    /// Lịch sử cập nhật odometer (km) của xe
    /// </summary>
    public class OdometerHistory : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }

        public int OdometerValue { get; set; }

        public DateOnly RecordedDate { get; set; }

        public OdometerSource Source { get; set; } = OdometerSource.ManualInput;

        // Navigation properties
        public UserVehicle UserVehicle { get; set; } = null!;
    }
}
