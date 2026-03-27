namespace Verendar.Garage.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.OwnsOne(e => e.BookedTotalPrice);

        builder.HasMany(e => e.LineItems)
            .WithOne(i => i.Booking)
            .HasForeignKey(i => i.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BookingLineItemConfiguration : IEntityTypeConfiguration<BookingLineItem>
{
    public void Configure(EntityTypeBuilder<BookingLineItem> builder)
    {
        builder.OwnsOne(e => e.BookedItemPrice);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Service)
            .WithMany()
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Bundle)
            .WithMany()
            .HasForeignKey(e => e.BundleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
