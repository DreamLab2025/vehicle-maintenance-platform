namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageMemberConfiguration : IEntityTypeConfiguration<GarageMember>
{
    public void Configure(EntityTypeBuilder<GarageMember> builder)
    {
        builder.HasIndex(m => new { m.GarageBranchId, m.UserId })
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");
    }
}
