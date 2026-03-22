namespace Verendar.Location.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;

public class LocationDbContext(DbContextOptions<LocationDbContext> options) : DbContext(options)
{
    public DbSet<AdministrativeRegion> AdministrativeRegions { get; set; } = null!;
    public DbSet<AdministrativeUnit> AdministrativeUnits { get; set; } = null!;
    public DbSet<Province> Provinces { get; set; } = null!;
    public DbSet<Ward> Wards { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocationDbContext).Assembly);
    }
}
