using Microsoft.EntityFrameworkCore;
using Verendar.Notification.Domain.Entities;
using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Infrastructure.Data
{
    public static class NotificationDataSeeder
    {
        public static void SeedNotificationData(this ModelBuilder modelBuilder)
        {
            var fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var templates = new List<NotificationTemplate>
            {
                new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Code = "OTP_PHONE_VERIFICATION", NotificationType = NotificationType.System, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "Ma xac thuc SDT", MessageTemplate = "Ma xac thuc: {OTP}. Het han sau {ExpiryMinutes}p." },
                new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Code = "OTP_PASSWORD_RESET", NotificationType = NotificationType.System, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "Ma dat lai mat khau", MessageTemplate = "Ma reset pass: {OTP}. Het han sau {ExpiryMinutes}p." },
                new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Code = "OTP_TWO_FACTOR", NotificationType = NotificationType.System, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "Ma 2FA", MessageTemplate = "Ma 2FA: {OTP}." },
                new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Code = "OTP_REQUESTED", NotificationType = NotificationType.System, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "Ma OTP", MessageTemplate = "Ma OTP: {OTP}." },
                new() { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Code = "WELCOME_USER", NotificationType = NotificationType.Welcome, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "Chao mung!", MessageTemplate = "Xin chao {UserName} den voi Verender!" },
                new() { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), Code = "PHONE_VERIFIED", NotificationType = NotificationType.Welcome, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "SDT da xac thuc", MessageTemplate = "SDT {PhoneNumber} da xac thuc." },
                new() { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), Code = "PASSWORD_CHANGED", NotificationType = NotificationType.System, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "Doi mat khau", MessageTemplate = "Mat khau da doi luc {ChangeTime}." },
                new() { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), Code = "ACCOUNT_LOCKED", NotificationType = NotificationType.System, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "Tai khoan bi khoa", MessageTemplate = "Ly do: {Reason}." },
                new() { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), Code = "ACCOUNT_UNLOCKED", NotificationType = NotificationType.System, IsActive = true, CreatedAt = fixedDate, TitleTemplate = "Mo khoa tai khoan", MessageTemplate = "Tai khoan da duoc mo." }
            };

            var channels = new List<NotificationTemplateChannel>();
            int channelIdCounter = 1;

            foreach (var t in templates)
            {
                channels.Add(CreateChannel(ref channelIdCounter, t.Id, NotificationChannel.SMS));
            }

            modelBuilder.Entity<NotificationTemplate>().HasData(templates);
            modelBuilder.Entity<NotificationTemplateChannel>().HasData(channels);
        }

        private static NotificationTemplateChannel CreateChannel(ref int counter, Guid templateId, NotificationChannel channel)
        {
            return new NotificationTemplateChannel
            {
                Id = Guid.Parse($"90000000-0000-0000-0000-{counter++:D12}"),
                NotificationTemplateId = templateId,
                Channel = channel,
                IsEnabled = true
            };
        }
    }
}