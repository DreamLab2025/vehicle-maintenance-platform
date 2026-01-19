using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Infrastructure.Data.Configurations
{
    public class VehiclePartCategoryConfiguration : IEntityTypeConfiguration<VehiclePartCategory>
    {
        public void Configure(EntityTypeBuilder<VehiclePartCategory> builder)
        {
            builder.ToTable("VehiclePartCategories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.IconUrl)
                .HasMaxLength(200);

            // Unique index on Code
            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL")
                .HasDatabaseName("IX_VehiclePartCategories_Code_Unique");

            // Index for display order
            builder.HasIndex(c => new { c.DisplayOrder, c.Status })
                .HasFilter("\"DeletedAt\" IS NULL");

            // Relationships
            builder.HasMany(c => c.VehicleParts)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
