using Verendar.Vehicle.Contracts.Enums;

namespace Verendar.Notification.Application.Constants
{
    public static class NotificationConstants
    {
        public static class TemplateKeys
        {
            public const string Otp = "Otp";
            public const string Notification = "Notification";
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

        public static class ConsumerCopy
        {
            public const string BookingCreatedTitle = "Đặt lịch thành công";
            public const string BookingCompletedTitle = "Bảo dưỡng hoàn tất";
            public const string BookingConfirmedTitle = "Lịch hẹn đã được xác nhận";
            public const string BookingCancelledTitle = "Lịch hẹn đã bị hủy";
            public const string BookingStatusChangedTitle = "Cập nhật trạng thái lịch hẹn";
            public const string BookingNewStaffTitle = "Có lịch đặt mới";
            public const string BookingAssignedMechanicTitle = "Bạn được phân công lịch hẹn";
            public const string BookingCompletedStaffTitle = "Lịch hẹn đã hoàn tất";
            public const string MaintenanceTitleUrgent = "Cần bảo dưỡng ngay";
            public const string MaintenanceTitleNormal = "Nhắc bảo dưỡng xe";
            public const string MaintenanceVehicleFallbackName = "xe của bạn";
            public const string EmailCtaViewApp = "Xem chi tiết trong app";
            public const string EmailCtaViewBooking = "Xem lịch hẹn";
            public const string EmailCtaViewDetail = "Xem chi tiết";
            public const string LoginCta = "Đăng nhập Verendar";
        }
    }
}
