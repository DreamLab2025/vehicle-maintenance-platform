namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class OdometerHistoryConfiguration : IEntityTypeConfiguration<OdometerHistory>
    {
        public void Configure(EntityTypeBuilder<OdometerHistory> builder)
        {
            builder.HasIndex(e => new { e.UserVehicleId, e.RecordedDate }).HasFilter("\"DeletedAt\" IS NULL");
        }
    }
}
