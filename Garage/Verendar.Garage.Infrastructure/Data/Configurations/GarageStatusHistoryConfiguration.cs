using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageStatusHistoryConfiguration : IEntityTypeConfiguration<GarageStatusHistory>
{
    public void Configure(EntityTypeBuilder<GarageStatusHistory> builder)
    {
        builder.HasOne(h => h.Garage)
            .WithMany()
            .HasForeignKey(h => h.GarageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
