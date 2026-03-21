using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class PartTrackingService(
        ILogger<PartTrackingService> logger,
        IUnitOfWork unitOfWork,
        IMaintenanceReminderService maintenanceReminderService) : IPartTrackingService
    {
        private readonly ILogger<PartTrackingService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMaintenanceReminderService _maintenanceReminderService = maintenanceReminderService;

        public async Task<ApiResponse<List<PartSummary>>> GetPartsByUserVehicleAsync(Guid userId, Guid userVehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<List<PartSummary>>.NotFoundResponse("Không tìm thấy xe");

                var trackings = await _unitOfWork.PartTrackings.GetByUserVehicleIdAsync(userVehicleId);
                var summaries = trackings.Select(t => t.ToPartSummary()).ToList();

                return ApiResponse<List<PartSummary>>.SuccessResponse(
                    summaries,
                    "Lấy danh sách phụ tùng xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts for user vehicle {UserVehicleId}", userVehicleId);
                return ApiResponse<List<PartSummary>>.FailureResponse("Lỗi khi lấy danh sách phụ tùng xe");
            }
        }

        public async Task<ApiResponse<List<TrackingCycleSummary>>> GetCyclesForPartAsync(Guid userId, Guid vehicleId, Guid partTrackingId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<List<TrackingCycleSummary>>.NotFoundResponse("Không tìm thấy xe");

                var partTracking = await _unitOfWork.PartTrackings
                    .FindOneAsync(t => t.Id == partTrackingId && t.UserVehicleId == vehicleId);

                if (partTracking == null)
                    return ApiResponse<List<TrackingCycleSummary>>.NotFoundResponse("Không tìm thấy phụ tùng");

                var cycles = await _unitOfWork.TrackingCycles.GetByPartTrackingIdAsync(partTrackingId);
                var summaries = cycles.Select(c => c.ToSummary(vehicle.CurrentOdometer)).ToList();

                return ApiResponse<List<TrackingCycleSummary>>.SuccessResponse(summaries, "Lấy danh sách tracking cycle thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cycles for part tracking {PartTrackingId}", partTrackingId);
                return ApiResponse<List<TrackingCycleSummary>>.FailureResponse("Lỗi khi lấy danh sách tracking cycle");
            }
        }

        public async Task<ApiResponse<PartTrackingSummary>> ApplyTrackingConfigAsync(Guid userId, Guid vehicleId, ApplyTrackingConfigRequest request)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.Variant)
                        .ThenInclude(vv => vv.VehicleModel)
                    .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<PartTrackingSummary>.NotFoundResponse("Không tìm thấy xe");

                var partCategory = await _unitOfWork.PartCategories.AsQueryable()
                    .FirstOrDefaultAsync(pc => pc.Code == request.PartCategoryCode);

                if (partCategory == null)
                    return ApiResponse<PartTrackingSummary>.NotFoundResponse(
                        $"Không tìm thấy linh kiện với mã '{request.PartCategoryCode}'");

                var existingTracking = await _unitOfWork.PartTrackings.AsQueryable()
                    .Include(t => t.PartCategory)
                    .FirstOrDefaultAsync(t => t.UserVehicleId == vehicleId && t.PartCategoryId == partCategory.Id);

                PartTracking tracking;

                if (existingTracking == null)
                {
                    tracking = vehicleId.ToPartTracking(partCategory.Id, request);
                    await _unitOfWork.PartTrackings.AddAsync(tracking);
                    await _unitOfWork.SaveChangesAsync();
                    tracking.PartCategory = partCategory;

                    _logger.LogInformation("Created new tracking for vehicle {VehicleId}, part {PartCode}", vehicleId, request.PartCategoryCode);
                }
                else
                {
                    existingTracking.ApplyTrackingConfig(request);
                    await _unitOfWork.PartTrackings.UpdateAsync(existingTracking.Id, existingTracking);
                    await _unitOfWork.SaveChangesAsync();
                    tracking = existingTracking;

                    _logger.LogInformation("Updated existing tracking for vehicle {VehicleId}, part {PartCode}", vehicleId, request.PartCategoryCode);
                }

                // Complete old cycle and resolve its reminders
                var existingCycle = await _unitOfWork.TrackingCycles.AsQueryable()
                    .Include(c => c.Reminders)
                    .FirstOrDefaultAsync(c => c.PartTrackingId == tracking.Id && c.Status == CycleStatus.Active);

                if (existingCycle != null)
                {
                    existingCycle.Status = CycleStatus.Completed;
                    foreach (var r in existingCycle.Reminders)
                        r.Status = ReminderStatus.Resolved;
                }

                // Start new cycle
                var newCycle = new TrackingCycle
                {
                    PartTrackingId = tracking.Id,
                    StartOdometer = vehicle.CurrentOdometer,
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TargetOdometer = request.PredictedNextOdometer,
                    TargetDate = request.PredictedNextDate,
                    Status = CycleStatus.Active,
                };
                await _unitOfWork.TrackingCycles.AddAsync(newCycle);
                await _unitOfWork.SaveChangesAsync();

                // Sync will lazily create first reminder for the new cycle
                await _maintenanceReminderService.SyncRemindersAsync(vehicleId, vehicle.CurrentOdometer, userId);
                await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);

                return ApiResponse<PartTrackingSummary>.SuccessResponse(
                    tracking.ToSummary(vehicle.CurrentOdometer),
                    "Áp dụng cấu hình theo dõi thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying tracking config for vehicle {VehicleId}, part {PartCode}", vehicleId, request.PartCategoryCode);
                return ApiResponse<PartTrackingSummary>.FailureResponse("Lỗi khi áp dụng cấu hình theo dõi");
            }
        }

        public async Task InitializeForVehicleAsync(Guid userVehicleId, Guid vehicleModelId)
        {
            var schedules = await _unitOfWork.DefaultSchedules.GetByVehicleModelIdAsync(vehicleModelId);
            var partCategoryIds = schedules
                .Where(s => s.PartCategoryId != Guid.Empty)
                .Select(s => s.PartCategoryId)
                .Distinct()
                .ToList();

            foreach (var partCategoryId in partCategoryIds)
            {
                var tracking = userVehicleId.ToInitializePartTracking(partCategoryId);
                await _unitOfWork.PartTrackings.AddAsync(tracking);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
