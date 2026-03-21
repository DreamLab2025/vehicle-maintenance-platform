namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
    {
        public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
        {
            builder.HasIndex(e => new { e.UserVehicleId, e.ServiceDate }).HasFilter("\"DeletedAt\" IS NULL");
            builder.Property(r => r.TotalCost).HasColumnType("decimal(18,2)");
        }
    }
}
