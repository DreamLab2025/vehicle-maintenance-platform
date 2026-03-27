namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageServiceConfiguration : IEntityTypeConfiguration<GarageService>
{
    public void Configure(EntityTypeBuilder<GarageService> builder)
    {
        builder.OwnsOne(e => e.LaborPrice);

        builder.HasOne(e => e.ServiceCategory)
            .WithMany(c => c.Services)
            .HasForeignKey(e => e.ServiceCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
