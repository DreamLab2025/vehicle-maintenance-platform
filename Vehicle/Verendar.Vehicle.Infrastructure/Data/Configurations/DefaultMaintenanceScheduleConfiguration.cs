namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class DefaultMaintenanceScheduleConfiguration : IEntityTypeConfiguration<DefaultMaintenanceSchedule>
    {
        public void Configure(EntityTypeBuilder<DefaultMaintenanceSchedule> builder)
        {
            builder.HasIndex(e => new { e.VehicleModelId, e.PartCategoryId }).HasFilter("\"DeletedAt\" IS NULL");
        }
    }
}
