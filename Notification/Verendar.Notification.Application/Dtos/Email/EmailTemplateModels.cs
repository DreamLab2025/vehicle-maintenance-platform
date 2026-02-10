namespace Verendar.Notification.Application.Dtos.Email
{
    // Base model for all email templates
    public abstract class EmailTemplateModel
    {
        public string UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

    // Welcome email template model
    public class WelcomeEmailModel : EmailTemplateModel
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string? WelcomeMessage { get; set; }
    }

    // OTP email template model
    public class OtpEmailModel : EmailTemplateModel
    {
        public string OtpCode { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; }
        public DateTime ExpiryTime { get; set; }
        public string OtpType { get; set; } = string.Empty; // Login, Registration, PasswordReset, etc.
    }

    // Password reset email template model
    public class PasswordResetEmailModel : EmailTemplateModel
    {
        public string ResetToken { get; set; } = string.Empty;
        public string ResetUrl { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    // Generic notification email model
    public class NotificationEmailModel : EmailTemplateModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
    }

    // Nhắc nhở thay linh kiện / bảo dưỡng
    public class MaintenanceReminderEmailModel : EmailTemplateModel
    {
        public string Title { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
        public List<MaintenanceReminderItemEmailDto> Items { get; set; } = [];
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
    }

    public class MaintenanceReminderItemEmailDto
    {
        public string PartCategoryName { get; set; } = string.Empty;
        public string? VehicleDisplayName { get; set; }
        public int CurrentOdometer { get; set; }
        public int TargetOdometer { get; set; }
        public decimal PercentageRemaining { get; set; }
    }

    // Nhắc nhở cập nhật số km
    public class OdometerReminderEmailModel : EmailTemplateModel
    {
        public string Title { get; set; } = string.Empty;
        public int StaleOdometerDays { get; set; }
        public List<OdometerReminderVehicleEmailDto> Vehicles { get; set; } = [];
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
    }

    public class OdometerReminderVehicleEmailDto
    {
        public string VehicleDisplayName { get; set; } = string.Empty;
        public string? LicensePlate { get; set; }
        public int CurrentOdometer { get; set; }
        public string? LastOdometerUpdateFormatted { get; set; }
        public int DaysSinceUpdate { get; set; }
    }

    // Order confirmation email model (example for future use)
    public class OrderConfirmationEmailModel : EmailTemplateModel
    {
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
