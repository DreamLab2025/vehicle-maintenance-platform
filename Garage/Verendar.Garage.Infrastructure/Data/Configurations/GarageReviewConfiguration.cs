namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class GarageReviewConfiguration : IEntityTypeConfiguration<GarageReview>
{
    public void Configure(EntityTypeBuilder<GarageReview> builder)
    {
        builder.HasIndex(e => e.BookingId)
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");
    }
}
