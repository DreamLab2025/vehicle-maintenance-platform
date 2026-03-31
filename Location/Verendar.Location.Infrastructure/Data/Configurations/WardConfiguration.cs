namespace Verendar.Location.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> builder)
    {
        // String PK: Code
        builder.HasKey(e => e.Code);
        builder.Property(e => e.Code).HasMaxLength(10);
        builder.Property(e => e.Name).HasMaxLength(500).IsRequired();
        builder.Property(e => e.ProvinceCode).HasMaxLength(10);
        builder.Property(e => e.BoundaryUrl).HasMaxLength(500).IsRequired(false);

        // Foreign key to Province (by code)
        builder.HasOne(e => e.Province)
            .WithMany(p => p.Wards)
            .HasForeignKey(e => e.ProvinceCode)
            .HasPrincipalKey(p => p.Code)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to AdministrativeUnit
        builder.HasOne(e => e.AdministrativeUnit)
            .WithMany()
            .HasForeignKey(e => e.AdministrativeUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(e => e.ProvinceCode);
        builder.HasIndex(e => e.AdministrativeUnitId);
        builder.HasIndex(e => e.Name);
    }
}
