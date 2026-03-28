using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class MaintenanceRecordService(
        IUnitOfWork unitOfWork,
        IMaintenanceReminderService maintenanceReminderService,
        ILogger<MaintenanceRecordService> logger) : IMaintenanceRecordService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMaintenanceReminderService _maintenanceReminderService = maintenanceReminderService;
        private readonly ILogger<MaintenanceRecordService> _logger = logger;

        public async Task<ApiResponse<CreateRecordResponse>> CreateMaintenanceRecordAsync(Guid userId, Guid vehicleId, CreateRecordRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                _logger.LogWarning("CreateMaintenanceRecord: no items vehicle {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<CreateRecordResponse>.FailureResponse("Cần ít nhất một phụ tùng thay thế trong phiếu bảo dưỡng");
            }

            var vehicle = await _unitOfWork.UserVehicles.GetByIdAndUserIdWithoutPartTrackingsAsync(vehicleId, userId);

            if (vehicle == null)
            {
                _logger.LogWarning("CreateMaintenanceRecord: vehicle not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<CreateRecordResponse>.NotFoundResponse("Không tìm thấy xe");
            }

            var record = request.ToMaintenanceRecord(vehicleId);
            await _unitOfWork.MaintenanceRecords.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            int lastOdo = record.OdometerAtService;
            DateOnly lastDate = record.ServiceDate;
            var itemResults = new List<RecordItemResult>();
            var trackingIdsToResetReminders = new List<Guid>();
            var trackingIdsForResponse = new List<Guid>();

            var requestedSlugs = request.Items.Select(i => i.PartCategorySlug).Distinct().ToList();
            var partCategoriesMap = (await _unitOfWork.PartCategories.GetBySlugsAsync(requestedSlugs))
                .ToDictionary(pc => pc.Slug, StringComparer.OrdinalIgnoreCase);

            if (record.OdometerAtService >= vehicle.CurrentOdometer)
            {
                var previousOdo = vehicle.CurrentOdometer;
                vehicle.UpdateOdometer(record.OdometerAtService);
                vehicle.LastOdometerUpdate = record.ServiceDate;
                await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);

                var odometerHistory = new OdometerHistory
                {
                    UserVehicleId = vehicleId,
                    OdometerValue = record.OdometerAtService,
                    RecordedDate = record.ServiceDate,
                    Source = OdometerSource.ServiceRecord,
                    KmOnRecordedDate = record.OdometerAtService - previousOdo
                };
                await _unitOfWork.OdometerHistories.AddAsync(odometerHistory);
                await _unitOfWork.SaveChangesAsync();
            }

            foreach (var itemInput in request.Items)
            {
                if (!partCategoriesMap.TryGetValue(itemInput.PartCategorySlug, out var partCategory))
                {
                    _logger.LogWarning("CreateMaintenanceRecord: part category slug not found {PartCategorySlug} vehicle {VehicleId}", itemInput.PartCategorySlug, vehicleId);
                    return ApiResponse<CreateRecordResponse>.NotFoundResponse(
                        $"Không tìm thấy linh kiện với slug '{itemInput.PartCategorySlug}'");
                }

                var existingTracking = await _unitOfWork.PartTrackings.GetByUserVehicleAndPartCategoryAsync(
                    vehicleId,
                    partCategory.Id,
                    itemInput.InstanceIdentifier);

                int? predictedNextOdo = null;
                DateOnly? predictedNextDate = null;
                int? customKm = null;
                int? customMonths = null;
                Guid? currentGarageProductId = itemInput.GarageProductId;

                if (itemInput.UpdatesTracking)
                {
                    customKm = itemInput.CustomKmInterval;
                    customMonths = itemInput.CustomMonthsInterval;
                    if (customKm.GetValueOrDefault(0) > 0) predictedNextOdo = lastOdo + (customKm ?? 0);
                    if (customMonths.GetValueOrDefault(0) > 0) predictedNextDate = lastDate.AddMonths(customMonths ?? 0);
                    if (existingTracking != null && !predictedNextOdo.HasValue && !predictedNextDate.HasValue)
                    {
                        customKm = existingTracking.CustomKmInterval ?? customKm;
                        customMonths = existingTracking.CustomMonthsInterval ?? customMonths;
                        predictedNextOdo = existingTracking.PredictedNextOdometer;
                        predictedNextDate = existingTracking.PredictedNextDate;
                    }
                }
                else if (existingTracking != null)
                {
                    customKm = existingTracking.CustomKmInterval;
                    customMonths = existingTracking.CustomMonthsInterval;
                    predictedNextOdo = existingTracking.PredictedNextOdometer;
                    predictedNextDate = existingTracking.PredictedNextDate;
                }

                var item = itemInput.ToMaintenanceRecordItem(record.Id, partCategory.Id, currentGarageProductId);
                await _unitOfWork.MaintenanceRecordItems.AddAsync(item);

                PartTracking tracking;
                if (existingTracking == null)
                {
                    tracking = MaintenanceRecordMappings.ToNewVehiclePartTracking(
                        vehicleId,
                        partCategory.Id,
                        itemInput.InstanceIdentifier,
                        lastOdo,
                        lastDate,
                        predictedNextOdo,
                        predictedNextDate,
                        customKm,
                        customMonths,
                        currentGarageProductId);
                    await _unitOfWork.PartTrackings.AddAsync(tracking);
                    tracking.PartCategory = partCategory;
                    trackingIdsForResponse.Add(tracking.Id);
                }
                else
                {
                    existingTracking.ApplyMaintenanceRecordUpdate(
                        currentGarageProductId,
                        itemInput.InstanceIdentifier,
                        lastOdo,
                        lastDate,
                        predictedNextOdo,
                        predictedNextDate,
                        customKm,
                        customMonths);
                    await _unitOfWork.PartTrackings.UpdateAsync(existingTracking.Id, existingTracking);
                    tracking = existingTracking;
                    trackingIdsToResetReminders.Add(tracking.Id);
                    trackingIdsForResponse.Add(tracking.Id);
                }

                var trackingSummary = tracking.ToSummary(vehicle.CurrentOdometer);
                itemResults.Add(MaintenanceRecordMappings.ToRecordItemResult(item.Id, itemInput.PartCategorySlug, trackingSummary));
            }

            await _unitOfWork.SaveChangesAsync();

            foreach (var trackingId in trackingIdsToResetReminders)
            {
                var cycles = await _unitOfWork.TrackingCycles.GetActiveWithRemindersByPartTrackingIdAsync(trackingId);
                foreach (var cycle in cycles)
                {
                    cycle.Status = CycleStatus.Completed;
                    foreach (var r in cycle.Reminders)
                    {
                        r.Status = ReminderStatus.Resolved;
                        await _unitOfWork.MaintenanceReminders.UpdateAsync(r.Id, r);
                    }
                }
            }
            if (trackingIdsToResetReminders.Count > 0)
                await _unitOfWork.SaveChangesAsync();

            await _maintenanceReminderService.SyncRemindersAsync(vehicleId, vehicle.CurrentOdometer, userId);
            await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);

            if (trackingIdsForResponse.Count > 0)
            {
                var trackingsWithReminders = await _unitOfWork.PartTrackings.GetByIdsWithCyclesAndRemindersAsync(trackingIdsForResponse);
                var trackingMap = trackingsWithReminders.ToDictionary(t => t.Id);
                for (var i = 0; i < itemResults.Count && i < trackingIdsForResponse.Count; i++)
                {
                    var tid = trackingIdsForResponse[i];
                    if (trackingMap.TryGetValue(tid, out var fresh) && fresh != null)
                        itemResults[i].Tracking = fresh.ToSummary(vehicle.CurrentOdometer);
                }
            }

            var response = MaintenanceRecordMappings.ToCreateRecordResponse(record.Id, itemResults);
            return ApiResponse<CreateRecordResponse>.CreatedResponse(response, "Tạo phiếu bảo dưỡng thành công");
        }

        public async Task<ApiResponse<IReadOnlyList<RecordSummaryDto>>> GetMaintenanceHistoryAsync(Guid userId, Guid userVehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles.FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);
            if (vehicle == null)
            {
                _logger.LogWarning("GetMaintenanceHistory: vehicle not found {UserVehicleId} user {UserId}", userVehicleId, userId);
                return ApiResponse<IReadOnlyList<RecordSummaryDto>>.NotFoundResponse("Không tìm thấy xe");
            }

            var records = await _unitOfWork.MaintenanceRecords.GetByUserVehicleIdWithItemsAsync(userVehicleId);
            var list = records.Select(r => r.ToRecordSummaryDto()).ToList();
            return ApiResponse<IReadOnlyList<RecordSummaryDto>>.SuccessResponse(list, "Lấy lịch sử bảo dưỡng thành công");
        }

        public async Task<ApiResponse<RecordDetailDto>> GetMaintenanceRecordDetailAsync(Guid userId, Guid maintenanceRecordId)
        {
            var record = await _unitOfWork.MaintenanceRecords.GetWithItemsAsync(maintenanceRecordId);
            if (record == null)
            {
                _logger.LogWarning("GetMaintenanceRecordDetail: record not found {MaintenanceRecordId}", maintenanceRecordId);
                return ApiResponse<RecordDetailDto>.NotFoundResponse("Không tìm thấy phiếu bảo dưỡng");
            }

            var vehicle = await _unitOfWork.UserVehicles.FindOneAsync(v => v.Id == record.UserVehicleId && v.UserId == userId);
            if (vehicle == null)
            {
                _logger.LogWarning("GetMaintenanceRecordDetail: forbidden user {UserId} record {MaintenanceRecordId} vehicle {UserVehicleId}", userId, maintenanceRecordId, record.UserVehicleId);
                return ApiResponse<RecordDetailDto>.ForbiddenResponse("Bạn không có quyền xem phiếu bảo dưỡng này");
            }

            var detail = record.ToRecordDetailDto();
            return ApiResponse<RecordDetailDto>.SuccessResponse(detail, "Lấy chi tiết phiếu bảo dưỡng thành công");
        }
    }
}
