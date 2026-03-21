namespace Verendar.Vehicle.Domain.Entities
{
    public class DefaultMaintenanceSchedule : BaseEntity
    {
        [Required]
        public Guid VehicleModelId { get; set; }

        [Required]
        public Guid PartCategoryId { get; set; }

        public int InitialKm { get; set; }

        public int KmInterval { get; set; }

        public int MonthsInterval { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        public Model VehicleModel { get; set; } = null!;
        public PartCategory PartCategory { get; set; } = null!;
    }
}
