namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(UserVehicleId), nameof(RecordedDate))]
    public class OdometerHistory : BaseEntity
    {
        [Required]
        public Guid UserVehicleId { get; set; }

        public int OdometerValue { get; set; }

        public DateOnly RecordedDate { get; set; }

        public OdometerSource Source { get; set; } = OdometerSource.ManualInput;

        public int? KmOnRecordedDate { get; set; }

        public UserVehicle UserVehicle { get; set; } = null!;
    }
}
