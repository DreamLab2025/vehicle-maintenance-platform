namespace Verendar.Location.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        // String PK: Code
        builder.HasKey(e => e.Code);
        builder.Property(e => e.Code).HasMaxLength(10);
        builder.Property(e => e.Name).HasMaxLength(500).IsRequired();
        builder.Property(e => e.BoundaryUrl).HasMaxLength(500).IsRequired(false);

        // Foreign key to AdministrativeRegion
        builder.HasOne(e => e.AdministrativeRegion)
            .WithMany()
            .HasForeignKey(e => e.AdministrativeRegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AdministrativeUnit)
            .WithMany()
            .HasForeignKey(e => e.AdministrativeUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(e => e.AdministrativeRegionId);
        builder.HasIndex(e => e.AdministrativeUnitId);
        builder.HasIndex(e => e.Name);
    }
}
