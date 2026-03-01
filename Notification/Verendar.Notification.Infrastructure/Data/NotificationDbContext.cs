using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Notification.Domain.Entities;

namespace Verendar.Notification.Infrastructure.Data
{
    public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : BaseDbContext(options)
    {
        public DbSet<Domain.Entities.Notification> Notifications { get; set; } = null!;
        public DbSet<NotificationDelivery> NotificationDeliveries { get; set; } = null!;
        public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
        public DbSet<NotificationTemplateChannel> NotificationTemplateChannels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.Notification>(entity =>
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
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
            modelBuilder.Entity<NotificationTemplateChannel>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
