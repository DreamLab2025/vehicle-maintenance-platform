using Verendar.Vehicle.Contracts.Enums;

namespace Verendar.Notification.Application.Constants
{
    public static class NotificationConstants
    {
        public static class TemplateKeys
        {
            public const string Otp = "Otp";
            public const string Welcome = "Welcome";
            public const string OdometerReminder = "OdometerReminder";
            public const string MaintenanceReminder = "MaintenanceReminder";
            public const string MemberAccountCreated = "MemberAccountCreated";
        }

        public static class MaintenanceLevelLabels
        {
            public const string Critical = "Khẩn cấp";
            public const string High = "Cao";
            public const string Medium = "Trung bình";
            public const string Low = "Thấp";
            public const string Normal = "Bình thường";

            public static string GetLabel(ReminderLevel level) => level switch
            {
                ReminderLevel.Critical => Critical,
                ReminderLevel.High => High,
                ReminderLevel.Medium => Medium,
                ReminderLevel.Low => Low,
                ReminderLevel.Normal => Normal,
                _ => level.ToString()
            };
        }

        public static class Titles
        {
            public const string Otp = "Mã xác thực OTP";
            public const string Welcome = "Chào mừng đến với Verendar!";
            public const string OdometerReminder = "Nhắc nhở cập nhật số km";
            public const string MemberAccountCreated = "Tài khoản Garage của bạn đã được tạo";
            public static readonly string MaintenanceCriticalPart = $"{MaintenanceLevelLabels.Critical}: Cần thay";
            public static readonly string MaintenanceCritical = $"{MaintenanceLevelLabels.Critical}: Cần thay linh kiện";
            public const string MaintenanceNormalPrefix = "Nhắc nhở bảo dưỡng";
        }

        public static class MetadataKeys
        {
            public const string TemplateKey = "TemplateKey";
        }

        public static class Defaults
        {
            public const int StaleOdometerDays = 3;
        }

        public static class DateFormats
        {
            public const string DateOnly = "dd/MM/yyyy";
            public const string DateTime = "HH:mm dd/MM/yyyy";
        }
    }
}
