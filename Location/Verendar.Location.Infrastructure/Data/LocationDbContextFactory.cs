namespace Verendar.Location.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class LocationDbContextFactory : IDesignTimeDbContextFactory<LocationDbContext>
{
    public LocationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LocationDbContext>();
        optionsBuilder.UseNpgsql("Server=localhost;Database=verendar_location;User Id=postgres;Password=postgres");

        return new LocationDbContext(optionsBuilder.Options);
    }
}
