using Verendar.Common.Jwt;
using GarageEntity = global::Verendar.Garage.Domain.Entities.Garage;
using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Infrastructure.Data;

public class GarageDbContext(DbContextOptions<GarageDbContext> options, ICurrentUserService? currentUserService = null)
    : BaseDbContext(options, currentUserService)
{
    public DbSet<GarageEntity> Garages { get; set; } = null!;
    public DbSet<GarageBranch> GarageBranches { get; set; } = null!;    
    public DbSet<GarageProduct> GarageProducts { get; set; } = null!;
    public DbSet<GarageMember> GarageMembers { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingStatusHistory> BookingStatusHistories { get; set; } = null!;
    public DbSet<GarageStatusHistory> GarageStatusHistories { get; set; } = null!;
    public DbSet<GarageReview> GarageReviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GarageDbContext).Assembly);
    }
}
