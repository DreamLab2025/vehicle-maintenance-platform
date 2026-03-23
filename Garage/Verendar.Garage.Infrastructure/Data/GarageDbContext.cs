using Verendar.Common.Jwt;

namespace Verendar.Garage.Infrastructure.Data;

public class GarageDbContext(DbContextOptions<GarageDbContext> options, ICurrentUserService? currentUserService = null)
    : BaseDbContext(options, currentUserService)
{
    public DbSet<GarageAccount> GarageAccounts { get; set; } = null!;
    public DbSet<GarageBranch> GarageBranches { get; set; } = null!;
    public DbSet<GarageProduct> GarageProducts { get; set; } = null!;
    public DbSet<Mechanic> Mechanics { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingStatusHistory> BookingStatusHistories { get; set; } = null!;
    public DbSet<GarageReview> GarageReviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GarageDbContext).Assembly);
    }
}
