namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class ModelConfiguration : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder.HasIndex(e => e.Code).HasFilter("\"DeletedAt\" IS NULL");
            builder.HasIndex(e => e.VehicleBrandId).HasFilter("\"DeletedAt\" IS NULL");

            builder.HasOne(m => m.Brand)
                .WithMany(b => b.VehicleModels)
                .HasForeignKey(m => m.VehicleBrandId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(m => m.EngineCapacity).HasColumnType("decimal(4,2)");
        }
    }
}
