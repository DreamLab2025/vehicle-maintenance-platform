using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Infrastructure.Data.Configurations
{
    public class OilConfiguration : IEntityTypeConfiguration<Oil>
    {
        public void Configure(EntityTypeBuilder<Oil> builder)
        {
            builder.ToTable("Oils");

            builder.HasKey(o => o.Id);

            // Mỗi Oil gắn với đúng 1 VehiclePart
            builder.HasIndex(o => o.VehiclePartId)
                .IsUnique();

            builder.Property(o => o.ViscosityGrade)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.ApiServiceClass)
                .HasMaxLength(20);

            builder.Property(o => o.JasoRating)
                .HasMaxLength(10);

            builder.Property(o => o.BaseOilType)
                .HasMaxLength(50);

            builder.Property(o => o.RecommendedVolumeLiters)
                .HasColumnType("decimal(6,2)");

            // Quan hệ 1-1 với VehiclePart
            builder.HasOne(o => o.VehiclePart)
                .WithOne()
                .HasForeignKey<Oil>(o => o.VehiclePartId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
