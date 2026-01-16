using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verender.Media.Infrastructure.Data
{
    public class MediaDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
    {
        public MediaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MediaDbContext>();

            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=MediaDb;Username=postgres;Password=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(MediaDbContext).Assembly.FullName));

            return new MediaDbContext(optionsBuilder.Options);
        }
    }
}
