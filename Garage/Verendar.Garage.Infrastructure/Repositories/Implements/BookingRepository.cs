namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class BookingRepository(GarageDbContext context)
    : PostgresRepository<Booking>(context), IBookingRepository
{
}
