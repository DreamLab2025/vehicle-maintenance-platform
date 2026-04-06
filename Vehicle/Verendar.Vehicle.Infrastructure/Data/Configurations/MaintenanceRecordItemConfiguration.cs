namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class MaintenanceRecordItemConfiguration : IEntityTypeConfiguration<MaintenanceRecordItem>
    {
        public void Configure(EntityTypeBuilder<MaintenanceRecordItem> builder)
        {
            builder.Property(i => i.Price).HasColumnType("decimal(18,2)");
        }
    }
}
