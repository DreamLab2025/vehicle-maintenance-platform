using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace VMP.Common.Bootstrapping
{
    public static class DatabaseApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddPostgresDatabase<TContext>(
            this IHostApplicationBuilder builder,
            string databaseName)
            where TContext : DbContext
        {
            builder.AddNpgsqlDbContext<TContext>(databaseName, configureDbContextOptions: options =>
            {
                options.UseNpgsql(builder => builder.MigrationsAssembly(typeof(TContext).Assembly.FullName));
            });

            return builder;
        }
    }
}
