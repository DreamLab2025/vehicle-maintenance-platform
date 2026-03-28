using MassTransit;
using Verendar.Vehicle.Application.Dtos.Internal;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Contracts.Events;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class OdometerHistoryService(
        ILogger<OdometerHistoryService> logger,
        IUnitOfWork unitOfWork,
        IMaintenanceReminderService maintenanceReminderService,
        IPublishEndpoint publishEndpoint) : IOdometerHistoryService
    {
        private readonly ILogger<OdometerHistoryService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMaintenanceReminderService _maintenanceReminderService = maintenanceReminderService;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

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

                await PublishOdometerUpdatedAsync(vehicleId, userId, request.CurrentOdometer);
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
                    var odometerHistory = vehicleId.ToPhotoInputOdometerHistory(request.ConfirmedOdometer, vehicle.CurrentOdometer, request.MediaFileId);
                    await _unitOfWork.OdometerHistories.AddAsync(odometerHistory);

                    vehicle.UpdateOdometer(request.ConfirmedOdometer);
                    await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);

                    await _maintenanceReminderService.SyncRemindersAsync(vehicleId, request.ConfirmedOdometer, userId);
                });

                await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);

                await PublishOdometerUpdatedAsync(vehicleId, userId, request.ConfirmedOdometer);
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

        public async Task<ApiResponse<OdometerHistorySummaryDto>> GetSummaryAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));

            var last3MonthsTask = _unitOfWork.OdometerHistories.GetRecordedOnOrAfterOrderedAsync(userVehicleId, cutoff, cancellationToken);
            var allTask = _unitOfWork.OdometerHistories.GetAllByUserVehicleIdAsync(userVehicleId, cancellationToken);

            await Task.WhenAll(last3MonthsTask, allTask);

            var last3Months = last3MonthsTask.Result;
            var all = allTask.Result;

            return ApiResponse<OdometerHistorySummaryDto>.SuccessResponse(new OdometerHistorySummaryDto
            {
                EntryCount = all.Count,
                KmPerMonthAvg = ComputeKmPerMonth(all),
                KmPerMonthLast3Months = ComputeKmPerMonth(last3Months)
            });
        }

        private async Task PublishOdometerUpdatedAsync(Guid vehicleId, Guid userId, int newOdometerValue)
        {
            try
            {
                var totalEntryCount = (int)await _unitOfWork.OdometerHistories.CountAsync(h => h.UserVehicleId == vehicleId);

                await _publishEndpoint.Publish(new OdometerUpdatedEvent
                {
                    UserVehicleId = vehicleId,
                    UserId = userId,
                    NewOdometerValue = newOdometerValue,
                    RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TotalEntryCount = totalEntryCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PublishOdometerUpdated: failed to publish event for vehicle {VehicleId}", vehicleId);
            }
        }

        private static double? ComputeKmPerMonth(IReadOnlyList<Domain.Entities.OdometerHistory> records)
        {
            if (records.Count < 2) return null;

            var ordered = records.OrderBy(r => r.RecordedDate).ToList();
            var first = ordered.First();
            var last = ordered.Last();

            var days = last.RecordedDate.DayNumber - first.RecordedDate.DayNumber;
            if (days <= 0) return null;

            var kmDiff = last.OdometerValue - first.OdometerValue;
            if (kmDiff <= 0) return null;

            return Math.Round(kmDiff / (days / 30.0), 2);
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
