namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class PartCategoryConfiguration : IEntityTypeConfiguration<PartCategory>
    {
        public void Configure(EntityTypeBuilder<PartCategory> builder)
        {
            builder.HasIndex(e => e.Code).HasFilter("\"DeletedAt\" IS NULL");
            builder.HasIndex(e => e.DisplayOrder).HasFilter("\"DeletedAt\" IS NULL");
        }
    }
}
