namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.HasIndex(e => e.Slug).HasFilter("\"DeletedAt\" IS NULL");
        }
    }
}
