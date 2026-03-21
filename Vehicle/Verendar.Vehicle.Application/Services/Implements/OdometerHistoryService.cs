using Microsoft.EntityFrameworkCore;
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

        public async Task<ApiResponse<UserVehicleResponse>> UpdateOdometerAsync(Guid userId, Guid vehicleId, UpdateOdometerRequest request)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<UserVehicleResponse>.NotFoundResponse("Không tìm thấy xe");

                if (request.CurrentOdometer < vehicle.CurrentOdometer)
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Số km mới phải lớn hơn hoặc bằng số km hiện tại");

                if (request.CurrentOdometer != vehicle.CurrentOdometer)
                {
                    var oldOdometer = vehicle.CurrentOdometer;

                    await _unitOfWork.BeginTransactionAsync();

                    var odometerHistory = vehicleId.ToOdometerHistory(request.CurrentOdometer, oldOdometer);
                    await _unitOfWork.OdometerHistories.AddAsync(odometerHistory);

                    vehicle.UpdateOdometer(request.CurrentOdometer);
                    await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);

                    await _maintenanceReminderService.SyncRemindersAsync(vehicleId, request.CurrentOdometer, userId);

                    await _unitOfWork.CommitTransactionAsync();

                    await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);

                    _logger.LogInformation("Updated odometer for vehicle: {VehicleId} from {OldOdometer} to {NewOdometer} km",
                        vehicleId, oldOdometer, request.CurrentOdometer);
                }

                var updatedVehicle = await _unitOfWork.UserVehicles.GetByIdWithFullDetailsAsync(vehicleId);

                return ApiResponse<UserVehicleResponse>.SuccessResponse(
                    updatedVehicle!.ToResponse(),
                    "Cập nhật số km thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating odometer for vehicle: {VehicleId}", vehicleId);
                return ApiResponse<UserVehicleResponse>.FailureResponse("Lỗi khi cập nhật số km");
            }
        }

        public async Task<ApiResponse<StreakResponse>> GetVehicleStreakAsync(Guid userId, Guid userVehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

            if (vehicle == null)
                return ApiResponse<StreakResponse>.NotFoundResponse("Không tìm thấy xe");

            var streak = await _unitOfWork.OdometerHistories.GetCurrentStreakAsync(userVehicleId);
            return ApiResponse<StreakResponse>.SuccessResponse(streak.ToStreakResponse(userVehicleId), "Lấy chuỗi xe thành công");
        }

        public async Task<ApiResponse<List<OdometerHistoryItemDto>>> GetOdometerHistoryPagedAsync(Guid userId, Guid userVehicleId, OdometerHistoryQueryRequest query)
        {
            try
            {
                query.Normalize();

                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<List<OdometerHistoryItemDto>>.NotFoundResponse("Không tìm thấy xe");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting odometer history for user vehicle {UserVehicleId}", userVehicleId);
                return ApiResponse<List<OdometerHistoryItemDto>>.FailureResponse("Lỗi khi lấy lịch sử số km");
            }
        }
    }
}
