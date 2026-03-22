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
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetPartsByUserVehicle: vehicle not found {UserVehicleId} user {UserId}", userVehicleId, userId);
                return ApiResponse<List<PartSummary>>.NotFoundResponse("Không tìm thấy xe");
            }

            var trackings = await _unitOfWork.PartTrackings.GetByUserVehicleIdAsync(userVehicleId);
            var summaries = trackings.Select(t => t.ToPartSummary()).ToList();

            return ApiResponse<List<PartSummary>>.SuccessResponse(
                summaries,
                "Lấy danh sách phụ tùng xe thành công");
        }

        public async Task<ApiResponse<List<TrackingCycleSummary>>> GetCyclesForPartAsync(Guid userId, Guid vehicleId, Guid partTrackingId)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetCyclesForPart: vehicle not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<List<TrackingCycleSummary>>.NotFoundResponse("Không tìm thấy xe");
            }

            var partTracking = await _unitOfWork.PartTrackings
                .FindOneAsync(t => t.Id == partTrackingId && t.UserVehicleId == vehicleId);

            if (partTracking == null)
            {
                _logger.LogWarning("GetCyclesForPart: part tracking not found {PartTrackingId} vehicle {VehicleId}", partTrackingId, vehicleId);
                return ApiResponse<List<TrackingCycleSummary>>.NotFoundResponse("Không tìm thấy phụ tùng");
            }

            var cycles = await _unitOfWork.TrackingCycles.GetByPartTrackingIdAsync(partTrackingId);
            var summaries = cycles.Select(c => c.ToSummary(vehicle.CurrentOdometer)).ToList();

            return ApiResponse<List<TrackingCycleSummary>>.SuccessResponse(summaries, "Lấy danh sách tracking cycle thành công");
        }

        public async Task<ApiResponse<PartTrackingSummary>> ApplyTrackingConfigAsync(Guid userId, Guid vehicleId, ApplyTrackingConfigRequest request)
        {
            var vehicle = await _unitOfWork.UserVehicles.GetByIdAndUserIdWithoutPartTrackingsAsync(vehicleId, userId);

            if (vehicle == null)
            {
                _logger.LogWarning("ApplyTrackingConfig: vehicle not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<PartTrackingSummary>.NotFoundResponse("Không tìm thấy xe");
            }

            var partCategory = await _unitOfWork.PartCategories.GetBySlugAsync(request.PartCategorySlug);

            if (partCategory == null)
            {
                _logger.LogWarning("ApplyTrackingConfig: part category slug not found {PartCategorySlug} vehicle {VehicleId}", request.PartCategorySlug, vehicleId);
                return ApiResponse<PartTrackingSummary>.NotFoundResponse(
                    $"Không tìm thấy linh kiện với slug '{request.PartCategorySlug}'");
            }

            var existingTracking = await _unitOfWork.PartTrackings.GetFirstByUserVehicleAndPartCategoryAsync(vehicleId, partCategory.Id);

            PartTracking tracking = null!;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (existingTracking == null)
                {
                    tracking = vehicleId.ToPartTracking(partCategory.Id, request);
                    await _unitOfWork.PartTrackings.AddAsync(tracking);
                }
                else
                {
                    existingTracking.ApplyTrackingConfig(request);
                    await _unitOfWork.PartTrackings.UpdateAsync(existingTracking.Id, existingTracking);
                    tracking = existingTracking;
                }

                var activeCycles = await _unitOfWork.TrackingCycles.GetActiveWithRemindersByPartTrackingIdAsync(tracking.Id);
                var existingCycle = activeCycles.FirstOrDefault();

                if (existingCycle != null)
                {
                    existingCycle.Status = CycleStatus.Completed;
                    foreach (var r in existingCycle.Reminders)
                        r.Status = ReminderStatus.Resolved;
                }

                await _unitOfWork.TrackingCycles.AddAsync(new TrackingCycle
                {
                    PartTrackingId = tracking.Id,
                    StartOdometer = vehicle.CurrentOdometer,
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TargetOdometer = request.PredictedNextOdometer,
                    TargetDate = request.PredictedNextDate,
                    Status = CycleStatus.Active,
                });
            });

            if (existingTracking == null)
                tracking.PartCategory = partCategory;

            await _maintenanceReminderService.SyncRemindersAsync(vehicleId, vehicle.CurrentOdometer, userId);
            await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);

            return ApiResponse<PartTrackingSummary>.SuccessResponse(
                tracking.ToSummary(vehicle.CurrentOdometer),
                "Áp dụng cấu hình theo dõi thành công");
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
