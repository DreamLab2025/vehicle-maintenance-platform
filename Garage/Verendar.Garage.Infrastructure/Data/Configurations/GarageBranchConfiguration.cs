using System.Text.Json;
using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageBranchConfiguration : IEntityTypeConfiguration<GarageBranch>
{
    public void Configure(EntityTypeBuilder<GarageBranch> builder)
    {
        builder.OwnsOne(e => e.Address);

        builder.Property(e => e.WorkingHours)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<WorkingHours>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("jsonb");
    }
}
