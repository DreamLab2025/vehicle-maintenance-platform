using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class OdometerHistoryService(
        ILogger<OdometerHistoryService> logger,
        IUnitOfWork unitOfWork,
        IMaintenanceReminderService maintenanceReminderService) : IOdometerHistoryService
    {
        private readonly ILogger<OdometerHistoryService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMaintenanceReminderService _maintenanceReminderService = maintenanceReminderService;

        public async Task<ApiResponse<UpdateOdometerResponse>> UpdateOdometerAsync(Guid userId, Guid vehicleId, UpdateOdometerRequest request)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("UpdateOdometer: vehicle not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<UpdateOdometerResponse>.NotFoundResponse("Không tìm thấy xe");
            }

            if (request.CurrentOdometer < vehicle.CurrentOdometer)
            {
                _logger.LogWarning("UpdateOdometer: invalid odometer {NewOdo} < current {CurrentOdo} vehicle {VehicleId}", request.CurrentOdometer, vehicle.CurrentOdometer, vehicleId);
                return ApiResponse<UpdateOdometerResponse>.FailureResponse("Số km mới phải lớn hơn hoặc bằng số km hiện tại");
            }

            if (request.CurrentOdometer != vehicle.CurrentOdometer)
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var odometerHistory = vehicleId.ToOdometerHistory(request.CurrentOdometer, vehicle.CurrentOdometer);
                    await _unitOfWork.OdometerHistories.AddAsync(odometerHistory);

                    vehicle.UpdateOdometer(request.CurrentOdometer);
                    await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);

                    await _maintenanceReminderService.SyncRemindersAsync(vehicleId, request.CurrentOdometer, userId);
                });

                await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);
            }

            return ApiResponse<UpdateOdometerResponse>.SuccessResponse(
                vehicle.ToUpdateOdometerResponse(),
                "Cập nhật số km thành công");
        }

        public async Task<ApiResponse<UpdateOdometerResponse>> FromScanOdometerAsync(Guid userId, Guid vehicleId, FromScanOdometerRequest request, CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("FromScanOdometer: vehicle not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<UpdateOdometerResponse>.NotFoundResponse("Không tìm thấy xe");
            }

            if (request.ConfirmedOdometer < vehicle.CurrentOdometer)
            {
                _logger.LogWarning("FromScanOdometer: invalid odometer {NewOdo} < current {CurrentOdo} vehicle {VehicleId}", request.ConfirmedOdometer, vehicle.CurrentOdometer, vehicleId);
                return ApiResponse<UpdateOdometerResponse>.FailureResponse("Số km xác nhận phải lớn hơn hoặc bằng số km hiện tại");
            }

            if (request.ConfirmedOdometer != vehicle.CurrentOdometer)
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var odometerHistory = vehicleId.ToPhotoInputOdometerHistory(request.ConfirmedOdometer, vehicle.CurrentOdometer);
                    await _unitOfWork.OdometerHistories.AddAsync(odometerHistory);

                    vehicle.UpdateOdometer(request.ConfirmedOdometer);
                    await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);

                    await _maintenanceReminderService.SyncRemindersAsync(vehicleId, request.ConfirmedOdometer, userId);
                });

                await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);
            }

            return ApiResponse<UpdateOdometerResponse>.SuccessResponse(
                vehicle.ToUpdateOdometerResponse(),
                "Cập nhật số km từ ảnh thành công");
        }

        public async Task<ApiResponse<StreakResponse>> GetVehicleStreakAsync(Guid userId, Guid userVehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetVehicleStreak: vehicle not found {UserVehicleId} user {UserId}", userVehicleId, userId);
                return ApiResponse<StreakResponse>.NotFoundResponse("Không tìm thấy xe");
            }

            var streak = await _unitOfWork.OdometerHistories.GetCurrentStreakAsync(userVehicleId);
            return ApiResponse<StreakResponse>.SuccessResponse(streak.ToStreakResponse(userVehicleId), "Lấy chuỗi xe thành công");
        }

        public async Task<ApiResponse<List<OdometerHistoryItemDto>>> GetOdometerHistoryPagedAsync(Guid userId, Guid userVehicleId, OdometerHistoryQueryRequest query)
        {
            query.Normalize();

            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetOdometerHistoryPaged: vehicle not found {UserVehicleId} user {UserId}", userVehicleId, userId);
                return ApiResponse<List<OdometerHistoryItemDto>>.NotFoundResponse("Không tìm thấy xe");
            }

            var isDescending = query.IsDescending ?? true;
            var (items, totalCount) = await _unitOfWork.OdometerHistories.GetPagedByUserVehicleAsync(
                userVehicleId,
                query.PageNumber,
                query.PageSize,
                query.FromDate,
                query.ToDate,
                isDescending);

            var dtos = items.Select(h => h.ToOdometerHistoryItemDto()).ToList();

            return ApiResponse<List<OdometerHistoryItemDto>>.SuccessPagedResponse(
                dtos,
                totalCount,
                query.PageNumber,
                query.PageSize,
                "Lấy lịch sử số km thành công");
        }
    }
}
