using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
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
            try
            {
                if (request.Items == null || request.Items.Count == 0)
                    return ApiResponse<CreateRecordResponse>.FailureResponse("Cần ít nhất một phụ tùng thay thế trong phiếu bảo dưỡng");

                var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.Variant)
                        .ThenInclude(vv => vv.VehicleModel)
                    .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<CreateRecordResponse>.NotFoundResponse("Không tìm thấy xe");

                var record = request.ToMaintenanceRecord(vehicleId);
                await _unitOfWork.MaintenanceRecords.AddAsync(record);
                await _unitOfWork.SaveChangesAsync();

                int lastOdo = record.OdometerAtService;
                DateOnly lastDate = record.ServiceDate;
                var itemResults = new List<RecordItemResult>();
                var trackingIdsToResetReminders = new List<Guid>();
                var trackingIdsForResponse = new List<Guid>();

                // Pre-load all part categories and products to avoid N+1 in the item loop
                var requestedCodes = request.Items.Select(i => i.PartCategoryCode).Distinct().ToList();
                var partCategoriesMap = (await _unitOfWork.PartCategories.AsQueryable()
                    .Where(pc => requestedCodes.Contains(pc.Code))
                    .ToListAsync())
                    .ToDictionary(pc => pc.Code, StringComparer.OrdinalIgnoreCase);

                var requestedProductIds = request.Items
                    .Where(i => i.UpdatesTracking && i.PartProductId.HasValue)
                    .Select(i => i.PartProductId!.Value)
                    .Distinct()
                    .ToList();
                var partProductsMap = requestedProductIds.Count > 0
                    ? (await _unitOfWork.PartProducts.AsQueryable()
                        .Where(p => requestedProductIds.Contains(p.Id))
                        .ToListAsync())
                        .ToDictionary(p => p.Id)
                    : new Dictionary<Guid, PartProduct>();

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
                    if (!partCategoriesMap.TryGetValue(itemInput.PartCategoryCode, out var partCategory))
                        return ApiResponse<CreateRecordResponse>.NotFoundResponse(
                            $"Không tìm thấy linh kiện với mã '{itemInput.PartCategoryCode}'");

                    var existingTracking = await _unitOfWork.PartTrackings.AsQueryable()
                        .Include(t => t.PartCategory)
                        .Include(t => t.CurrentPartProduct)
                        .FirstOrDefaultAsync(t => t.UserVehicleId == vehicleId
                            && t.PartCategoryId == partCategory.Id
                            && t.InstanceIdentifier == itemInput.InstanceIdentifier);

                    bool useProductForTracking = itemInput.UpdatesTracking && itemInput.PartProductId.HasValue;
                    int? predictedNextOdo = null;
                    DateOnly? predictedNextDate = null;
                    int? customKm = null;
                    int? customMonths = null;
                    Guid? currentPartProductId = itemInput.PartProductId;

                    if (useProductForTracking)
                    {
                        if (!partProductsMap.TryGetValue(itemInput.PartProductId!.Value, out var product)
                            || product.PartCategoryId != partCategory.Id)
                            return ApiResponse<CreateRecordResponse>.NotFoundResponse(
                                $"Phụ tùng không tồn tại hoặc không thuộc danh mục '{itemInput.PartCategoryCode}'");

                        customKm = product.RecommendedKmInterval;
                        customMonths = product.RecommendedMonthsInterval;
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
                    else if (itemInput.UpdatesTracking && (!itemInput.PartProductId.HasValue))
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

                    var item = itemInput.ToMaintenanceRecordItem(record.Id, partCategory.Id, currentPartProductId);
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
                            currentPartProductId);
                        await _unitOfWork.PartTrackings.AddAsync(tracking);
                        tracking.PartCategory = partCategory;
                        trackingIdsForResponse.Add(tracking.Id);
                    }
                    else
                    {
                        existingTracking.ApplyMaintenanceRecordUpdate(
                            currentPartProductId,
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
                    itemResults.Add(MaintenanceRecordMappings.ToRecordItemResult(item.Id, itemInput.PartCategoryCode, trackingSummary));
                }

                await _unitOfWork.SaveChangesAsync();

                foreach (var trackingId in trackingIdsToResetReminders)
                {
                    var cycles = await _unitOfWork.TrackingCycles.AsQueryable()
                        .Include(c => c.Reminders)
                        .Where(c => c.PartTrackingId == trackingId && c.Status == CycleStatus.Active)
                        .ToListAsync();
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
                    var trackingsWithReminders = await _unitOfWork.PartTrackings.AsQueryable()
                        .Where(t => trackingIdsForResponse.Contains(t.Id))
                        .Include(t => t.PartCategory)
                        .Include(t => t.CurrentPartProduct)
                        .Include(t => t.Cycles).ThenInclude(c => c.Reminders)
                        .ToListAsync();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance record for vehicle {VehicleId}", vehicleId);
                return ApiResponse<CreateRecordResponse>.FailureResponse("Lỗi khi tạo phiếu bảo dưỡng");
            }
        }

        public async Task<ApiResponse<IReadOnlyList<RecordSummaryDto>>> GetMaintenanceHistoryAsync(Guid userId, Guid userVehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                .FirstOrDefaultAsync(v => v.Id == userVehicleId && v.UserId == userId);
            if (vehicle == null)
                return ApiResponse<IReadOnlyList<RecordSummaryDto>>.NotFoundResponse("Không tìm thấy xe");

            var records = await _unitOfWork.MaintenanceRecords.GetByUserVehicleIdWithItemsAsync(userVehicleId);
            var list = records.Select(r => r.ToRecordSummaryDto()).ToList();
            return ApiResponse<IReadOnlyList<RecordSummaryDto>>.SuccessResponse(list, "Lấy lịch sử bảo dưỡng thành công");
        }

        public async Task<ApiResponse<RecordDetailDto>> GetMaintenanceRecordDetailAsync(Guid userId, Guid maintenanceRecordId)
        {
            var record = await _unitOfWork.MaintenanceRecords.GetWithItemsAsync(maintenanceRecordId);
            if (record == null)
                return ApiResponse<RecordDetailDto>.NotFoundResponse("Không tìm thấy phiếu bảo dưỡng");

            var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                .FirstOrDefaultAsync(v => v.Id == record.UserVehicleId && v.UserId == userId);
            if (vehicle == null)
                return ApiResponse<RecordDetailDto>.ForbiddenResponse("Bạn không có quyền xem phiếu bảo dưỡng này");

            var detail = record.ToRecordDetailDto();
            return ApiResponse<RecordDetailDto>.SuccessResponse(detail, "Lấy chi tiết phiếu bảo dưỡng thành công");
        }
    }
}
