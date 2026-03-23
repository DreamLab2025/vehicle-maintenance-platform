using Microsoft.EntityFrameworkCore.Design;

namespace Verendar.Garage.Infrastructure.Data;

public class GarageDbContextFactory : IDesignTimeDbContextFactory<GarageDbContext>
{
    public GarageDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GarageDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=verendar_garage;Username=postgres;Password=postgres",
            npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(GarageDbContext).Assembly.FullName));

        return new GarageDbContext(optionsBuilder.Options);
    }
}
