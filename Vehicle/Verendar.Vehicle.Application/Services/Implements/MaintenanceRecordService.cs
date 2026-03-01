using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Databases.Base;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class MaintenanceRecordService(
        IUnitOfWork unitOfWork,
        IUserVehicleService userVehicleService,
        ILogger<MaintenanceRecordService> logger) : IMaintenanceRecordService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUserVehicleService _userVehicleService = userVehicleService;
        private readonly ILogger<MaintenanceRecordService> _logger = logger;

        public async Task<ApiResponse<CreateMaintenanceRecordResponse>> CreateMaintenanceRecordAsync(Guid userId, Guid vehicleId, CreateMaintenanceRecordRequest request)
        {
            try
            {
                if (request.Items == null || request.Items.Count == 0)
                    return ApiResponse<CreateMaintenanceRecordResponse>.FailureResponse("Cần ít nhất một phụ tùng thay thế trong phiếu bảo dưỡng");

                var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.Variant)
                        .ThenInclude(vv => vv.VehicleModel)
                    .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<CreateMaintenanceRecordResponse>.FailureResponse("Không tìm thấy xe");

                var record = request.ToMaintenanceRecord(vehicleId);
                await _unitOfWork.MaintenanceRecords.AddAsync(record);
                await _unitOfWork.SaveChangesAsync();

                int lastOdo = record.OdometerAtService;
                DateOnly lastDate = record.ServiceDate;
                var itemResults = new List<CreateMaintenanceRecordItemResult>();
                var trackingIdsToResetReminders = new List<Guid>();

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
                    var partCategory = await _unitOfWork.PartCategories.AsQueryable()
                        .FirstOrDefaultAsync(pc => pc.Code == itemInput.PartCategoryCode);
                    if (partCategory == null)
                        return ApiResponse<CreateMaintenanceRecordResponse>.FailureResponse(
                            $"Không tìm thấy linh kiện với mã '{itemInput.PartCategoryCode}'");

                    var existingTracking = await _unitOfWork.VehiclePartTrackings.AsQueryable()
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
                        var product = await _unitOfWork.PartProducts.AsQueryable()
                            .FirstOrDefaultAsync(p => p.Id == itemInput.PartProductId!.Value && p.PartCategoryId == partCategory.Id);
                        if (product == null)
                            return ApiResponse<CreateMaintenanceRecordResponse>.FailureResponse(
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

                    VehiclePartTracking tracking;
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
                        await _unitOfWork.VehiclePartTrackings.AddAsync(tracking);
                        tracking.PartCategory = partCategory;
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
                        await _unitOfWork.VehiclePartTrackings.UpdateAsync(existingTracking.Id, existingTracking);
                        tracking = existingTracking;
                        trackingIdsToResetReminders.Add(tracking.Id);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    var trackingSummary = tracking.ToSummary(vehicle.CurrentOdometer);
                    itemResults.Add(MaintenanceRecordMappings.ToCreateMaintenanceRecordItemResult(item.Id, itemInput.PartCategoryCode, trackingSummary));
                }

                foreach (var trackingId in trackingIdsToResetReminders)
                {
                    var reminders = await _unitOfWork.MaintenanceReminders.AsQueryable()
                        .Where(r => r.VehiclePartTrackingId == trackingId)
                        .ToListAsync();
                    foreach (var r in reminders)
                    {
                        r.IsCurrent = false;
                        await _unitOfWork.MaintenanceReminders.UpdateAsync(r.Id, r);
                    }
                }
                if (trackingIdsToResetReminders.Count > 0)
                    await _unitOfWork.SaveChangesAsync();

                await _userVehicleService.SyncMaintenanceRemindersForVehicleAsync(vehicleId, vehicle.CurrentOdometer, userId);

                var response = MaintenanceRecordMappings.ToCreateMaintenanceRecordResponse(record.Id, itemResults);
                return ApiResponse<CreateMaintenanceRecordResponse>.SuccessResponse(response, "Tạo phiếu bảo dưỡng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance record for vehicle {VehicleId}", vehicleId);
                return ApiResponse<CreateMaintenanceRecordResponse>.FailureResponse("Lỗi khi tạo phiếu bảo dưỡng");
            }
        }

        public async Task<ApiResponse<IReadOnlyList<MaintenanceRecordSummaryDto>>> GetMaintenanceHistoryAsync(Guid userId, Guid userVehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                .FirstOrDefaultAsync(v => v.Id == userVehicleId && v.UserId == userId);
            if (vehicle == null)
                return ApiResponse<IReadOnlyList<MaintenanceRecordSummaryDto>>.FailureResponse("Không tìm thấy xe");

            var records = await _unitOfWork.MaintenanceRecords.GetByUserVehicleIdWithItemsAsync(userVehicleId);
            var list = records.Select(r => r.ToMaintenanceRecordSummaryDto()).ToList();
            return ApiResponse<IReadOnlyList<MaintenanceRecordSummaryDto>>.SuccessResponse(list, "Lấy lịch sử bảo dưỡng thành công");
        }

        public async Task<ApiResponse<MaintenanceRecordDetailDto>> GetMaintenanceRecordDetailAsync(Guid userId, Guid maintenanceRecordId)
        {
            var record = await _unitOfWork.MaintenanceRecords.GetWithItemsAsync(maintenanceRecordId);
            if (record == null)
                return ApiResponse<MaintenanceRecordDetailDto>.FailureResponse("Không tìm thấy phiếu bảo dưỡng");

            var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                .FirstOrDefaultAsync(v => v.Id == record.UserVehicleId && v.UserId == userId);
            if (vehicle == null)
                return ApiResponse<MaintenanceRecordDetailDto>.FailureResponse("Bạn không có quyền xem phiếu bảo dưỡng này");

            var detail = record.ToMaintenanceRecordDetailDto();
            return ApiResponse<MaintenanceRecordDetailDto>.SuccessResponse(detail, "Lấy chi tiết phiếu bảo dưỡng thành công");
        }
    }
}
