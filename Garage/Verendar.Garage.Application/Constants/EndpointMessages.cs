namespace Verendar.Garage.Application.Constants;

public static class EndpointMessages
{
    public static class Booking
    {
        public const string BranchNotFound = "Không tìm thấy chi nhánh. Bạn vui lòng kiểm tra lại hoặc chọn chi nhánh khác.";
        public const string GarageNotFound = "Không tìm thấy thông tin garage gắn với chi nhánh này.";
        public const string GarageInactive = "Garage hiện chưa mở cửa hoặc chưa được kích hoạt. Bạn vui lòng thử lại sau.";
        public const string BranchInactive = "Chi nhánh này hiện không nhận đặt lịch. Vui lòng chọn chi nhánh khác.";
        public const string EmptyItems = "Vui lòng chọn ít nhất một sản phẩm, dịch vụ hoặc gói combo.";
        public const string ScheduleMustBeFuture = "Thời gian hẹn cần nằm trong tương lai. Bạn vui lòng chọn lại.";
        public const string BookingCreated = "Đặt lịch thành công.";
        public const string BookingReloadFailed = "Đã tạo lịch hẹn nhưng chưa tải lại được chi tiết. Vui lòng thử mở lại sau vài giây.";
        public const string BookingNotFound = "Không tìm thấy lịch hẹn này.";
        public const string BookingForbiddenView = "Bạn không thể xem lịch hẹn này.";
        public const string BookingDetailSuccess = "Đã tải thông tin lịch hẹn.";
        public const string AssignedToMeConflict = "Không thể vừa lọc theo lịch của tôi vừa chọn thêm chi nhánh hoặc người dùng khác. Vui lòng bỏ bớt bộ lọc.";
        public const string BranchAndUserConflict = "Vui lòng chỉ chọn một: theo chi nhánh hoặc theo khách hàng, không chọn cả hai.";
        public const string MissingFilter = "Vui lòng chọn cách lọc: theo khách hàng, theo chi nhánh, hoặc lịch được giao cho bạn.";
        public const string UserMismatchForbidden = "Bạn chỉ có thể xem lịch hẹn của chính mình.";
        public const string BookingListSuccess = "Đã tải danh sách lịch hẹn.";
        public const string BranchBookingListSuccess = "Đã tải danh sách lịch hẹn theo chi nhánh.";
        public const string BranchBookingForbidden = "Bạn không thể xem danh sách lịch hẹn của chi nhánh này.";
        public const string NotMechanicForbidden = "Tài khoản của bạn chưa được gắn vai thợ máy hoặc chưa hoạt động.";
        public const string MechanicBookingListSuccess = "Đã tải danh sách lịch hẹn được giao cho bạn.";
        public const string AssignStatusInvalid = "Chỉ có thể gán thợ khi lịch hẹn đang chờ xử lý hoặc chờ xác nhận.";
        public const string AssignForbidden = "Bạn không thể gán thợ cho lịch hẹn này.";
        public const string MechanicNotFound = "Không tìm thấy thợ máy đang hoạt động tại chi nhánh này.";
        public const string AssignConcurrentlyModified = "Lịch hẹn đã được cập nhật. Vui lòng tải lại và thử lại.";
        public const string AssignSuccess = "Đã gán thợ và xác nhận lịch hẹn.";
        public const string NotAssignedYet = "Lịch hẹn này chưa được gán thợ.";
        public const string MechanicForbidden = "Bạn không phải thợ được gán cho lịch hẹn này.";
        public const string StartInvalid = "Chỉ có thể bắt đầu khi lịch hẹn đã được xác nhận.";
        public const string CompleteInvalid = "Chỉ có thể hoàn tất khi lịch hẹn đang được thực hiện.";
        public const string MechanicStatusInvalid = "Trạng thái cập nhật không phù hợp với bước hiện tại.";
        public const string MechanicStatusUpdated = "Đã cập nhật tiến độ lịch hẹn.";
        public const string CancelStatusInvalid = "Không thể hủy lịch hẹn ở trạng thái hiện tại.";
        public const string CancelForbidden = "Bạn không thể hủy lịch hẹn này.";
        public const string CancelSuccess = "Đã hủy lịch hẹn.";
        public const string VehicleNotFound = "Không tìm thấy xe này hoặc xe không thuộc tài khoản của bạn.";
    }

    public static class Review
    {
        public const string BookingNotFound = "Không tìm thấy lịch hẹn này.";
        public const string ReviewForbidden = "Bạn không thể đánh giá lịch hẹn này.";
        public const string BookingNotCompleted = "Chỉ có thể đánh giá sau khi lịch hẹn đã hoàn tất.";
        public const string AlreadyReviewed = "Lịch hẹn này đã được đánh giá.";
        public const string SubmitSuccess = "Cảm ơn bạn đã đánh giá.";
        public const string ReviewNotFound = "Chưa có đánh giá cho lịch hẹn này.";
        public const string GetReviewSuccess = "Đã tải đánh giá.";
        public const string GetBranchReviewsSuccess = "Đã tải danh sách đánh giá.";
    }

    public static class Stats
    {
        public const string GarageNotFound = "Không tìm thấy garage.";
        public const string BranchNotFound = "Không tìm thấy chi nhánh.";
        public const string Forbidden = "Bạn không thể xem thống kê này.";
        public const string GarageStatsSuccess = "Đã tải thống kê garage.";
        public const string BranchStatsSuccess = "Đã tải thống kê chi nhánh.";
        public const string PlatformStatsSuccess = "Đã tải thống kê toàn hệ thống.";
        public const string GarageDetailStatsSuccess = "Đã tải thống kê chi tiết garage.";
        public const string ChartSuccess = "Đã tải dữ liệu biểu đồ.";
    }

    public static class BranchManager
    {
        public const string BranchNotFoundByIdFormat = "Không tìm thấy chi nhánh (mã: {0}). Vui lòng kiểm tra lại.";
        public const string ForbiddenManageProducts = "Bạn không có quyền quản lý sản phẩm tại chi nhánh này.";
        public const string ForbiddenManageServices = "Bạn không có quyền quản lý dịch vụ tại chi nhánh này.";
        public const string ForbiddenManageBundles = "Bạn không có quyền quản lý gói combo tại chi nhánh này.";
    }

    public static class GarageBranches
    {
        public const string GarageNotFoundByIdFormat = "Không tìm thấy garage (mã: {0}). Vui lòng kiểm tra lại.";
        public const string ForbiddenAddBranch = "Bạn không thể thêm chi nhánh cho garage này.";
        public const string GarageNotApprovedAddBranch = "Garage chưa được duyệt nên chưa thể thêm chi nhánh. Vui lòng chờ xét duyệt.";
        public const string InvalidProvinceWard = "Tỉnh/thành hoặc phường/xã không đúng. Vui lòng chọn lại.";
        public const string CreateSuccess = "Đã tạo chi nhánh.";
        public const string GetDetailSuccess = "Đã tải thông tin chi nhánh.";
        public const string ListSuccess = "Đã tải danh sách chi nhánh.";
        public const string ForbiddenUpdateBranch = "Bạn không thể cập nhật chi nhánh của garage này.";
        public const string ForbiddenDeleteBranch = "Bạn không thể xóa chi nhánh của garage này.";
        public const string UpdateSuccess = "Đã cập nhật chi nhánh.";
        public const string DeleteSuccess = "Đã xóa chi nhánh.";
        public const string ForbiddenManageBranchStatus = "Bạn không thể thay đổi trạng thái chi nhánh của garage này.";
        public const string GarageNotApprovedBranchStatus = "Garage chưa được duyệt nên chưa thể đổi trạng thái chi nhánh.";
        public const string UpdateStatusSuccess = "Đã cập nhật trạng thái chi nhánh.";
        public const string GeocodeAddressNotFound = "Không tìm thấy vị trí theo địa chỉ đã nhập. Vui lòng thử địa chỉ khác.";
        public const string GeocodeBranchAddressFailed = "Không thể xác định tọa độ cho địa chỉ chi nhánh. Vui lòng kiểm tra lại địa chỉ và thử lại.";
        public const string MyBranchNoMembership = "Tài khoản chưa được gán vào chi nhánh đang hoạt động.";
    }

    public static class OwnerGarage
    {
        public const string ListSuccess = "Đã tải danh sách garage.";
        public const string MyGarageNotRegistered = "Bạn chưa đăng ký garage.";
        public const string GetDetailSuccess = "Đã tải thông tin garage.";
        public const string GarageNotFoundPlain = "Không tìm thấy garage.";
        public const string GarageNotFoundByIdFormat = "Không tìm thấy garage (mã: {0}). Vui lòng kiểm tra lại.";
        public const string StatusTransitionInvalidFormat = "Không thể chuyển từ trạng thái «{0}» sang «{1}».";
        public const string UpdateStatusSuccess = "Đã cập nhật trạng thái garage.";
        public const string CannotEditWhenActiveOrSuspended = "Không thể sửa thông tin khi garage đang hoạt động hoặc đang tạm ngưng.";
        public const string UpdateInfoSuccess = "Đã cập nhật thông tin garage.";
        public const string ResubmitOnlyWhenRejected = "Chỉ có thể gửi lại hồ sơ khi garage đang ở trạng thái bị từ chối.";
        public const string ResubmitSuccess = "Đã gửi lại hồ sơ để được xem xét.";
        public const string ConflictExistingGarage = "Tài khoản đã có garage. Nếu hồ sơ bị từ chối, vui lòng sửa thông tin rồi gửi lại.";
        public const string RejectedUseEditResubmitFlow = "Hồ sơ garage đã bị từ chối. Vui lòng cập nhật thông tin garage rồi dùng chức năng gửi lại hồ sơ.";
        public const string RegisterSuccess = "Đã gửi đăng ký garage.";
        public const string TaxLookupNotFoundFormat = "Không tìm thấy doanh nghiệp với MST '{0}'. Vui lòng kiểm tra lại.";
        public const string GarageNotFoundBySlugFormat = "Không tìm thấy garage với slug '{0}'. Vui lòng kiểm tra lại.";
    }

    public static class Member
    {
        public const string GarageNotFound = "Không tìm thấy garage.";
        public const string BranchNotInGarage = "Không tìm thấy chi nhánh thuộc garage này.";
        public const string ForbiddenAddMember = "Bạn không thể thêm thành viên.";
        public const string ManagerOnlyMechanic = "Quản lý chi nhánh chỉ có thể thêm thợ máy.";
        public const string IdentityCreateFailed = "Không tạo được tài khoản thành viên. Vui lòng thử lại sau.";
        public const string MemberAlreadyInBranch = "Thành viên này đã có trong chi nhánh.";
        public const string AddSuccess = "Đã thêm thành viên.";
        public const string ForbiddenListMembers = "Bạn không thể xem danh sách thành viên.";
        public const string ListSuccess = "Đã tải danh sách thành viên.";
        public const string MemberNotFound = "Không tìm thấy thành viên.";
        public const string MemberBranchNotFound = "Không tìm thấy chi nhánh của thành viên.";
        public const string ForbiddenUpdateMemberStatus = "Bạn không thể cập nhật trạng thái thành viên này.";
        public const string ManagerOnlyUpdateMechanic = "Quản lý chỉ có thể cập nhật thợ máy trong phạm vi chi nhánh của mình.";
        public const string UpdateStatusSuccess = "Đã cập nhật trạng thái thành viên.";
        public const string ForbiddenRemoveMember = "Bạn không thể xóa thành viên này.";
        public const string ManagerOnlyRemoveMechanic = "Quản lý chỉ có thể xóa thợ máy trong phạm vi chi nhánh của mình.";
        public const string RemoveSuccess = "Đã xóa thành viên.";
    }

    public static class ServiceCategory
    {
        public const string ListSuccess = "Đã tải danh mục dịch vụ.";
        public const string NotFoundByIdFormat = "Không tìm thấy danh mục (mã: {0}).";
        public const string GetSuccess = "Đã tải thông tin danh mục.";
        public const string SlugTakenFormat = "Đường dẫn «{0}» đã được sử dụng. Vui lòng chọn tên khác.";
        public const string CreateSuccess = "Đã tạo danh mục.";
        public const string UpdateSuccess = "Đã cập nhật danh mục.";
        public const string DeleteSuccess = "Đã xóa danh mục.";
    }

    public static class Product
    {
        public const string ListSuccess = "Đã tải danh sách sản phẩm.";
        public const string NotFoundByIdFormat = "Không tìm thấy sản phẩm (mã: {0}).";
        public const string GetSuccess = "Đã tải thông tin sản phẩm.";
        public const string InstallationServiceInvalid = "Dịch vụ lắp đặt không tồn tại hoặc không thuộc chi nhánh này.";
        public const string CreateSuccess = "Đã tạo sản phẩm.";
        public const string UpdateSuccess = "Đã cập nhật sản phẩm.";
        public const string UpdateStatusSuccess = "Đã cập nhật trạng thái sản phẩm.";
        public const string DeleteSuccess = "Đã xóa sản phẩm.";
    }

    public static class OfferedServices
    {
        public const string ListSuccess = "Đã tải danh sách dịch vụ.";
        public const string NotFoundByIdFormat = "Không tìm thấy dịch vụ (mã: {0}).";
        public const string GetSuccess = "Đã tải thông tin dịch vụ.";
        public const string CategoryNotFound = "Danh mục dịch vụ không tồn tại hoặc đã bị gỡ.";
        public const string CreateSuccess = "Đã tạo dịch vụ.";
        public const string UpdateSuccess = "Đã cập nhật dịch vụ.";
        public const string UpdateStatusSuccess = "Đã cập nhật trạng thái dịch vụ.";
        public const string DeleteSuccess = "Đã xóa dịch vụ.";
    }

    public static class Bundle
    {
        public const string ListSuccess = "Đã tải danh sách gói combo.";
        public const string NotFoundByIdFormat = "Không tìm thấy gói combo (mã: {0}).";
        public const string GetSuccess = "Đã tải thông tin gói combo.";
        public const string CreateSuccess = "Đã tạo gói combo.";
        public const string UpdateSuccess = "Đã cập nhật gói combo.";
        public const string UpdateStatusSuccess = "Đã cập nhật trạng thái gói combo.";
        public const string DeleteSuccess = "Đã xóa gói combo.";
        public const string EmptyItems = "Gói combo cần có ít nhất một mục.";
        public const string ItemSpecifyProductOrServiceFormat = "Mục #{0}: Chỉ chọn một — sản phẩm hoặc dịch vụ.";
        public const string ItemProductNotInBranchFormat = "Mục #{0}: Sản phẩm không tồn tại hoặc không thuộc chi nhánh này.";
        public const string ItemProductUnavailableFormat = "Mục #{0}: Sản phẩm «{1}» hiện không mở bán.";
        public const string ItemServiceNotInBranchFormat = "Mục #{0}: Dịch vụ không tồn tại hoặc không thuộc chi nhánh này.";
        public const string ItemServiceUnavailableFormat = "Mục #{0}: Dịch vụ «{1}» hiện không khả dụng.";
    }

    public static class Catalog
    {
        public const string BranchNotFoundByIdFormat = "Không tìm thấy chi nhánh (mã: {0}) hoặc chi nhánh hiện không hoạt động.";
        public const string ListSuccess = "Đã tải danh mục dịch vụ và sản phẩm.";
    }

    public static class BookingLineItem
    {
        public const string SpecifyOneFkFormat = "Mục #{0}: Chỉ chọn một trong sản phẩm, dịch vụ hoặc gói combo.";
        public const string ProductNotInBranchFormat = "Mục #{0}: Sản phẩm không tồn tại hoặc không thuộc chi nhánh này.";
        public const string ProductUnavailableFormat = "Mục #{0}: Sản phẩm «{1}» hiện không mở bán.";
        public const string ProductNoInstallationFormat = "Mục #{0}: Sản phẩm «{1}» không kèm dịch vụ lắp đặt.";
        public const string ServiceNotInBranchFormat = "Mục #{0}: Dịch vụ không tồn tại hoặc không thuộc chi nhánh này.";
        public const string ServiceUnavailableFormat = "Mục #{0}: Dịch vụ «{1}» hiện không khả dụng.";
        public const string BundleNotInBranchFormat = "Mục #{0}: Gói combo không tồn tại hoặc không thuộc chi nhánh này.";
        public const string BundleUnavailableFormat = "Mục #{0}: Gói combo «{1}» hiện không khả dụng.";
    }
}
