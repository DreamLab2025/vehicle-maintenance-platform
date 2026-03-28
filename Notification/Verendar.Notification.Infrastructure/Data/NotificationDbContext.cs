using Verendar.Common.Databases.Base;
using Verendar.Common.Jwt;
using NotificationEntity = Verendar.Notification.Domain.Entities.Notification;

namespace Verendar.Notification.Infrastructure.Data;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options, ICurrentUserService? currentUserService = null) : BaseDbContext(options, currentUserService)
{
    public DbSet<NotificationEntity> Notifications { get; set; } = null!;
    public DbSet<NotificationDelivery> NotificationDeliveries { get; set; } = null!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
        modelBuilder.Entity<NotificationDelivery>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        base.OnModelCreating(modelBuilder);
    }
}
