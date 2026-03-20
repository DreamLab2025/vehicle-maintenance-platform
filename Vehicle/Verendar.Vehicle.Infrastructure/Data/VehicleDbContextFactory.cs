using Microsoft.EntityFrameworkCore.Design;

namespace Verendar.Vehicle.Infrastructure.Data
{
    public class VehicleDbContextFactory : IDesignTimeDbContextFactory<VehicleDbContext>
    {
        public VehicleDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<VehicleDbContext>();

            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=VehicleDb;Username=postgres;Password=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(VehicleDbContext).Assembly.FullName));

            return new VehicleDbContext(optionsBuilder.Options);
        }
    }
}
