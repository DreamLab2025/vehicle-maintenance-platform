namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageProductConfiguration : IEntityTypeConfiguration<GarageProduct>
{
    public void Configure(EntityTypeBuilder<GarageProduct> builder)
    {
        builder.OwnsOne(e => e.Price);
    }
}
