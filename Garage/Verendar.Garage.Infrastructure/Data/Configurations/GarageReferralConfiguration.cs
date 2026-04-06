using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageReferralConfiguration : IEntityTypeConfiguration<GarageReferral>
{
    public void Configure(EntityTypeBuilder<GarageReferral> builder)
    {
        builder.HasOne(r => r.Garage)
            .WithMany()
            .HasForeignKey(r => r.GarageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.GarageId);
        builder.HasIndex(r => r.ReferredUserId);
        builder.HasIndex(r => new { r.GarageId, r.ReferredUserId }).IsUnique();
    }
}
