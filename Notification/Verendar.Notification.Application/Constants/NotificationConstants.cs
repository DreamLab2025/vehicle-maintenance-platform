namespace Verendar.Notification.Application.Constants
{
    public static class NotificationConstants
    {
        public static class TemplateKeys
        {
            public const string Otp = "Otp";
            public const string OdometerReminder = "OdometerReminder";
            public const string MaintenanceReminder = "MaintenanceReminder";
        }

        public static class Titles
        {
            public const string Otp = "Mã xác thực OTP";
            public const string OdometerReminder = "Nhắc nhở cập nhật số km";
            public const string MaintenanceCriticalPart = "Khẩn cấp: cần thay";
            public const string MaintenanceCritical = "Khẩn cấp: Cần thay linh kiện";
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
