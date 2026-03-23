namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.OwnsOne(e => e.BookedPrice);
    }
}
