using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Verendar.Ai.Infrastructure.Data
{

    public class AiDbContextFactory : IDesignTimeDbContextFactory<AiDbContext>
    {
        public AiDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AiDbContext>();

            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=AiDb;Username=postgres;Password=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(AiDbContext).Assembly.FullName));

            return new AiDbContext(optionsBuilder.Options);
        }
    }
}