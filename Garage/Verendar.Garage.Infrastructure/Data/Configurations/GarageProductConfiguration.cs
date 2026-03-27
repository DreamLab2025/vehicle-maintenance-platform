namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageProductConfiguration : IEntityTypeConfiguration<GarageProduct>
{
    public void Configure(EntityTypeBuilder<GarageProduct> builder)
    {
        builder.OwnsOne(e => e.MaterialPrice);

        builder.HasOne(e => e.InstallationService)
            .WithMany()
            .HasForeignKey(e => e.InstallationServiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
