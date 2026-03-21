namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class UserVehicleConfiguration : IEntityTypeConfiguration<UserVehicle>
    {
        public void Configure(EntityTypeBuilder<UserVehicle> builder)
        {
            builder.HasIndex(e => e.UserId).HasFilter("\"DeletedAt\" IS NULL");

            builder.HasOne(v => v.Variant)
                .WithMany()
                .HasForeignKey(v => v.VehicleVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
