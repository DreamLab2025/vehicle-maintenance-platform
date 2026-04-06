namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
    {
        public void Configure(EntityTypeBuilder<VehicleType> builder)
        {
            builder.HasIndex(e => e.Slug).HasFilter("\"DeletedAt\" IS NULL");
        }
    }
}
