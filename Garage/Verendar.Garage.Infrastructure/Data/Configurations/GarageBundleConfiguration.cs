namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageBundleConfiguration : IEntityTypeConfiguration<GarageBundle>
{
    public void Configure(EntityTypeBuilder<GarageBundle> builder)
    {
        builder.OwnsOne(e => e.DiscountAmount);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.GarageBundle)
            .HasForeignKey(i => i.GarageBundleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GarageBundleItemConfiguration : IEntityTypeConfiguration<GarageBundleItem>
{
    public void Configure(EntityTypeBuilder<GarageBundleItem> builder)
    {
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Service)
            .WithMany()
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
