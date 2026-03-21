namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class PartProductConfiguration : IEntityTypeConfiguration<PartProduct>
    {
        public void Configure(EntityTypeBuilder<PartProduct> builder)
        {
            builder.HasIndex(e => e.PartCategoryId).HasFilter("\"DeletedAt\" IS NULL");
            builder.Property(p => p.ReferencePrice).HasColumnType("decimal(18,2)");
        }
    }
}
