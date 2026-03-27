namespace Verendar.Garage.Application.Constants;

public static class EndpointMessages
{
    public static class Booking
    {
        public const string BranchNotFound = "Không tìm thấy chi nhánh. Vui lòng kiểm tra lại thông tin.";
        public const string GarageNotFound = "Không tìm thấy garage của chi nhánh này.";
        public const string GarageInactive = "Garage hiện chưa hoạt động, vui lòng thử lại sau.";
        public const string BranchInactive = "Chi nhánh hiện chưa hoạt động, vui lòng chọn chi nhánh khác.";
        public const string EmptyItems = "Vui lòng chọn ít nhất một sản phẩm, dịch vụ hoặc combo.";
        public const string ScheduleMustBeFuture = "Thời gian đặt lịch phải ở tương lai.";
        public const string BookingCreated = "Đặt lịch thành công.";
        public const string BookingReloadFailed = "Không thể tải booking vừa tạo. Vui lòng thử lại.";
        public const string BookingNotFound = "Không tìm thấy booking.";
        public const string BookingForbiddenView = "Bạn không có quyền xem booking này.";
        public const string BookingDetailSuccess = "Lấy thông tin booking thành công.";
        public const string AssignedToMeConflict = "Không thể kết hợp assignedToMe với branchId hoặc userId.";
        public const string BranchAndUserConflict = "Chỉ được chọn một trong branchId hoặc userId.";
        public const string MissingFilter = "Vui lòng cung cấp userId, branchId hoặc assignedToMe=true.";
        public const string UserMismatchForbidden = "Bạn chỉ có thể xem danh sách booking của chính mình.";
        public const string BookingListSuccess = "Lấy danh sách booking thành công.";
        public const string BranchBookingListSuccess = "Lấy danh sách booking theo chi nhánh thành công.";
        public const string BranchBookingForbidden = "Bạn không có quyền xem danh sách booking của chi nhánh này.";
        public const string NotMechanicForbidden = "Tài khoản chưa là thợ máy đang hoạt động.";
        public const string MechanicBookingListSuccess = "Lấy danh sách booking được giao thành công.";
        public const string AssignStatusInvalid = "Chỉ có thể gán thợ khi booking đang chờ xử lý.";
        public const string AssignForbidden = "Bạn không có quyền gán thợ cho booking này.";
        public const string MechanicNotFound = "Không tìm thấy thợ máy đang hoạt động thuộc chi nhánh này.";
        public const string AssignSuccess = "Đã gán thợ và xác nhận booking.";
        public const string NotAssignedYet = "Booking chưa được gán thợ.";
        public const string MechanicForbidden = "Bạn không phải thợ được gán cho booking này.";
        public const string StartInvalid = "Chỉ có thể bắt đầu khi booking đã được xác nhận.";
        public const string CompleteInvalid = "Chỉ có thể hoàn tất khi booking đang được thực hiện.";
        public const string MechanicStatusInvalid = "Trạng thái cập nhật không hợp lệ.";
        public const string MechanicStatusUpdated = "Cập nhật tiến độ booking thành công.";
        public const string CancelStatusInvalid = "Không thể hủy booking ở trạng thái hiện tại.";
        public const string CancelForbidden = "Bạn không có quyền hủy booking này.";
        public const string CancelSuccess = "Đã hủy booking.";
    }

    public static class Review
    {
        public const string BookingNotFound = "Không tìm thấy booking.";
        public const string ReviewForbidden = "Bạn không có quyền đánh giá booking này.";
        public const string BookingNotCompleted = "Chỉ có thể đánh giá sau khi booking đã hoàn tất.";
        public const string AlreadyReviewed = "Booking này đã được đánh giá.";
        public const string SubmitSuccess = "Đánh giá thành công.";
        public const string ReviewNotFound = "Chưa có đánh giá cho booking này.";
        public const string GetReviewSuccess = "Lấy đánh giá thành công.";
        public const string GetBranchReviewsSuccess = "Lấy danh sách đánh giá thành công.";
    }

    public static class Stats
    {
        public const string GarageNotFound = "Không tìm thấy garage.";
        public const string BranchNotFound = "Không tìm thấy chi nhánh.";
        public const string Forbidden = "Bạn không có quyền xem thống kê này.";
        public const string GarageStatsSuccess = "Lấy thống kê garage thành công.";
        public const string BranchStatsSuccess = "Lấy thống kê chi nhánh thành công.";
    }
}
