using Microsoft.EntityFrameworkCore.Design;

namespace Verendar.Notification.Infrastructure.Data
{
    public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=NotificationDb;Username=postgres;Password=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName));

            return new NotificationDbContext(optionsBuilder.Options);
        }
    }
}
