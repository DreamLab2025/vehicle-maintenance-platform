using Verendar.Garage.Contracts.Events;
using Verendar.Notification.Application.Constants;

namespace Verendar.Notification.Application.Mapping;

public static class GarageBookingNotificationMappings
{
    public static (string Title, string Body) BookingCreatedCopy(BookingCreatedEvent m)
    {
        var title = NotificationConstants.ConsumerCopy.BookingCreatedTitle;
        var body =
            $"Bạn đã đặt lịch tại {m.BranchName} — {m.ItemsSummary}. "
            + $"Lịch: {m.ScheduledAt:dd/MM/yyyy HH:mm} (UTC).";
        return (title, body);
    }

    public static (string Title, string Body) BookingCompletedCopy(BookingCompletedEvent m)
    {
        var title = NotificationConstants.ConsumerCopy.BookingCompletedTitle;
        var body = $"Xe của bạn vừa được bảo dưỡng tại {m.BranchName}. "
            + "Mở chi tiết lịch hẹn để xem thông tin.";
        return (title, body);
    }

    public static (string Title, string Body) BookingConfirmedCopy(BookingConfirmedEvent m)
    {
        var title = NotificationConstants.ConsumerCopy.BookingConfirmedTitle;
        var body = (
            $"Cơ sở {m.BranchName} xác nhận lịch {m.ScheduledAt:dd/MM/yyyy HH:mm} (UTC). "
            + $"Kỹ thuật viên: {m.MechanicDisplayName}. "
            + (string.IsNullOrWhiteSpace(m.ItemsSummary) ? "" : $"{m.ItemsSummary}. ")).Trim();
        return (title, body);
    }

    public static (string Title, string Body) BookingCancelledCopy(BookingCancelledEvent m)
    {
        var title = NotificationConstants.ConsumerCopy.BookingCancelledTitle;
        var reasonPart = string.IsNullOrWhiteSpace(m.Reason) ? "." : $" (lý do: {m.Reason}).";
        var body = $"Lịch tại {m.BranchName} đã bị hủy{reasonPart}";
        return (title, body);
    }

    public static (string Title, string Body) BookingStatusChangedCopy(BookingStatusChangedEvent m)
    {
        var title = NotificationConstants.ConsumerCopy.BookingStatusChangedTitle;
        var body = $"Lịch hẹn của bạn đã chuyển sang trạng thái {m.ToStatus}.";
        return (title, body);
    }
}
