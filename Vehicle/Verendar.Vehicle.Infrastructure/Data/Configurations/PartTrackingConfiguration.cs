namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class PartTrackingConfiguration : IEntityTypeConfiguration<PartTracking>
    {
        public void Configure(EntityTypeBuilder<PartTracking> builder)
        {
            builder.HasIndex(e => new { e.UserVehicleId, e.PartCategoryId, e.InstanceIdentifier }).HasFilter("\"DeletedAt\" IS NULL");
        }
    }
}
