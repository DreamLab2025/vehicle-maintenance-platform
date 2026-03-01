using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Seeders
{
    public static class EmailTemplateSeeder
    {
        private static readonly Guid SystemUserId = Guid.Empty;

        public static async Task SeedAsync(NotificationDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
        {
            foreach (var item in EmailTemplateSeedData.Items)
            {
                var existing = await db.NotificationTemplates
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.Code == item.Code, cancellationToken);

                if (existing != null)
                {
                    var changed = existing.TitleTemplate != item.TitleTemplate
                                  || existing.MessageTemplate != item.MessageTemplate;
                    if (changed)
                    {
                        existing.TitleTemplate = item.TitleTemplate;
                        existing.MessageTemplate = item.MessageTemplate;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedBy = SystemUserId;
                        logger?.LogInformation("Updated email template: {Code}", item.Code);
                    }

                    await EnsureEmailChannelExistsAsync(db, existing.Id, logger, cancellationToken);
                    continue;
                }

                var template = new NotificationTemplate
                {
                    Id = Guid.CreateVersion7(),
                    Code = item.Code,
                    TitleTemplate = item.TitleTemplate,
                    MessageTemplate = item.MessageTemplate,
                    NotificationType = item.NotificationType,
                    DefaultPriority = NotificationPriority.Medium,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = SystemUserId
                };
                db.NotificationTemplates.Add(template);

                var channel = new NotificationTemplateChannel
                {
                    Id = Guid.CreateVersion7(),
                    NotificationTemplateId = template.Id,
                    Channel = NotificationChannel.EMAIL,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = SystemUserId
                };
                db.NotificationTemplateChannels.Add(channel);

                logger?.LogInformation("Seeded email template: {Code}", item.Code);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureEmailChannelExistsAsync(
            NotificationDbContext db,
            Guid templateId,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            var hasEmailChannel = await db.NotificationTemplateChannels
                .IgnoreQueryFilters()
                .AnyAsync(c => c.NotificationTemplateId == templateId && c.Channel == NotificationChannel.EMAIL, cancellationToken);

            if (hasEmailChannel)
                return;

            var channel = new NotificationTemplateChannel
            {
                Id = Guid.CreateVersion7(),
                NotificationTemplateId = templateId,
                Channel = NotificationChannel.EMAIL,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SystemUserId
            };
            db.NotificationTemplateChannels.Add(channel);
            logger?.LogInformation("Added EMAIL channel for template Id: {TemplateId}", templateId);
        }
    }
}
