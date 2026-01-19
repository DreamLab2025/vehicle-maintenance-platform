using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Infrastructure.Data.Configurations
{
    public class VehiclePartConfiguration : IEntityTypeConfiguration<VehiclePart>
    {
        public void Configure(EntityTypeBuilder<VehiclePart> builder)
        {
            builder.ToTable("VehicleParts");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.Unit)
                .HasMaxLength(50);

            builder.Property(p => p.Sku)
                .HasMaxLength(100);

            builder.Property(p => p.ReferencePrice)
                .HasColumnType("decimal(18,2)");

            // Indexes
            builder.HasIndex(p => p.Name)
                .HasFilter("\"DeletedAt\" IS NULL");

            builder.HasIndex(p => p.Sku)
                .HasFilter("\"DeletedAt\" IS NULL AND \"Sku\" IS NOT NULL");

            builder.HasIndex(p => new { p.CategoryId, p.Status })
                .HasFilter("\"DeletedAt\" IS NULL");

            // Relationships
            builder.HasOne(p => p.Category)
                .WithMany(c => c.VehicleParts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
