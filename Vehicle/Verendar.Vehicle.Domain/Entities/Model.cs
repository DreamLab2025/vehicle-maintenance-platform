namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    [Index(nameof(VehicleBrandId))]
    public class Model : BaseEntity
    {
        [Required]
        public Guid VehicleBrandId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        public int? ManufactureYear { get; set; }

        public VehicleFuelType? FuelType { get; set; }

        public VehicleTransmissionType? TransmissionType { get; set; }

        public int? EngineDisplacement { get; set; }

        public decimal? EngineCapacity { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public Brand Brand { get; set; } = null!;
        public List<Variant> Variants { get; set; } = [];
        public List<DefaultMaintenanceSchedule> DefaultSchedules { get; set; } = [];
    }

    public enum VehicleFuelType
    {
        Gasoline = 1,

        Diesel = 2
    }

    public enum VehicleTransmissionType
    {
        Manual = 1,

        Automatic = 2,

        Sport = 3
    }
}
