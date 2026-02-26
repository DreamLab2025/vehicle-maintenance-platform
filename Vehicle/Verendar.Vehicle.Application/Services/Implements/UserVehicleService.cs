using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class UserVehicleService(
        ILogger<UserVehicleService> logger,
        IUnitOfWork unitOfWork,
        IMaintenanceReminderService maintenanceReminderService) : IUserVehicleService
    {
        private readonly ILogger<UserVehicleService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMaintenanceReminderService _maintenanceReminderService = maintenanceReminderService;

        public async Task<ApiResponse<IsAllowedToCreateVehicleResponse>> IsAllowedToCreateVehicleAsync(Guid userId)
        {
            try
            {
                var (isAllowed, message) = await _unitOfWork.UserVehicles.CheckCanCreateVehicleAsync(userId);
                return ApiResponse<IsAllowedToCreateVehicleResponse>.SuccessResponse(
                    isAllowed.ToIsAllowedToCreateVehicleResponse(message),
                    "Kiểm tra xem người dùng có được tạo xe mới không thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is allowed to create vehicle for user: {UserId}", userId);
                return ApiResponse<IsAllowedToCreateVehicleResponse>.FailureResponse("Lỗi khi kiểm tra xem người dùng có được tạo xe mới không");
            }
        }

        public async Task<ApiResponse<UserVehicleResponse>> CreateUserVehicleAsync(Guid userId, UserVehicleRequest request)
        {
            try
            {
                // Get vehicle variant with model info
                var vehicleVariant = await _unitOfWork.VehicleVariants.AsQueryable()
                    .Include(v => v.VehicleModel)
                    .FirstOrDefaultAsync(v => v.Id == request.VehicleVariantId);

                if (vehicleVariant == null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Phiên bản xe không tồn tại");
                }

                // var (isAllowed, message) = await _unitOfWork.UserVehicles
                //     .CheckCanCreateVehicleAsync(userId);

                // if (!isAllowed)
                // {
                //     return ApiResponse<UserVehicleResponse>.FailureResponse(message);
                // }

                var existingVehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.UserId == userId && v.LicensePlate == request.LicensePlate);

                if (existingVehicle != null)
                {
                    _logger.LogWarning("Attempt to create duplicate vehicle with license plate: {LicensePlate} for user: {UserId}",
                        request.LicensePlate, userId);
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Biển số xe đã tồn tại trong danh sách của bạn");
                }

                var userVehicle = request.ToEntity(userId);
                await _unitOfWork.UserVehicles.AddAsync(userVehicle);
                await _unitOfWork.SaveChangesAsync();

                // Create initial odometer history
                var initialOdometerHistory = userVehicle.Id.ToOdometerHistory(request.CurrentOdometer);

                await _unitOfWork.OdometerHistories.AddAsync(initialOdometerHistory);
                await _unitOfWork.SaveChangesAsync();

                await InitializePartTrackingAsync(userVehicle.Id);

                var createdVehicle = await _unitOfWork.UserVehicles.GetByIdWithFullDetailsAsync(userVehicle.Id);

                _logger.LogInformation("Created user vehicle with ID: {VehicleId} for user: {UserId}", userVehicle.Id, userId);

                return ApiResponse<UserVehicleResponse>.SuccessResponse(
                    createdVehicle!.ToResponse(),
                    "Thêm xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user vehicle for user: {UserId}", userId);
                return ApiResponse<UserVehicleResponse>.FailureResponse("Lỗi khi thêm xe");
            }
        }

        public async Task<ApiResponse<string>> DeleteUserVehicleAsync(Guid userId, Guid vehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm thấy xe");
                }

                var deletedAt = DateTime.UtcNow;

                var reminders = await _unitOfWork.MaintenanceReminders.GetByUserVehicleIdAsync(vehicleId);
                foreach (var r in reminders)
                {
                    r.DeletedAt = deletedAt;
                    r.DeletedBy = userId;
                    await _unitOfWork.MaintenanceReminders.UpdateAsync(r.Id, r);
                }

                var trackings = await _unitOfWork.VehiclePartTrackings.GetByUserVehicleIdAsync(vehicleId);
                foreach (var t in trackings)
                {
                    t.DeletedAt = deletedAt;
                    t.DeletedBy = userId;
                    await _unitOfWork.VehiclePartTrackings.UpdateAsync(t.Id, t);
                }

                var odometerHistories = await _unitOfWork.OdometerHistories.AsQueryable()
                    .Where(x => x.UserVehicleId == vehicleId)
                    .ToListAsync();
                foreach (var h in odometerHistories)
                {
                    h.DeletedAt = deletedAt;
                    h.DeletedBy = userId;
                    await _unitOfWork.OdometerHistories.UpdateAsync(h.Id, h);
                }

                var records = await _unitOfWork.MaintenanceRecords.GetByUserVehicleIdAsync(vehicleId);
                foreach (var record in records)
                {
                    var items = await _unitOfWork.MaintenanceRecordItems.GetByMaintenanceRecordIdAsync(record.Id);
                    foreach (var item in items)
                    {
                        item.DeletedAt = deletedAt;
                        item.DeletedBy = userId;
                        await _unitOfWork.MaintenanceRecordItems.UpdateAsync(item.Id, item);
                    }
                    record.DeletedAt = deletedAt;
                    record.DeletedBy = userId;
                    await _unitOfWork.MaintenanceRecords.UpdateAsync(record.Id, record);
                }

                vehicle.DeletedAt = deletedAt;
                vehicle.DeletedBy = userId;
                vehicle.Status = EntityStatus.Deleted;
                await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Soft deleted user vehicle with ID: {VehicleId} for user: {UserId} (cascade)", vehicleId, userId);

                return ApiResponse<string>.SuccessResponse(
                    "Deleted",
                    "Xóa xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user vehicle with ID: {VehicleId}", vehicleId);
                return ApiResponse<string>.FailureResponse("Lỗi khi xóa xe");
            }
        }

        public async Task<ApiResponse<UserVehicleDetailResponse>> GetUserVehicleByIdAsync(Guid userId, Guid vehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles.GetByIdAndUserIdWithoutPartTrackingsAsync(vehicleId, userId);

                if (vehicle == null)
                {
                    return ApiResponse<UserVehicleDetailResponse>.FailureResponse("Không tìm thấy xe");
                }

                var recordsQuery = _unitOfWork.MaintenanceRecords.AsQueryable()
                    .Where(r => r.UserVehicleId == vehicleId);
                var totalMaintenanceActivities = await recordsQuery.CountAsync();
                DateTime? lastMaintenanceDate = null;
                if (totalMaintenanceActivities > 0)
                {
                    var lastServiceDate = await recordsQuery.MaxAsync(r => r.ServiceDate);
                    lastMaintenanceDate = lastServiceDate.ToDateTime(TimeOnly.MinValue);
                }

                var response = vehicle.ToDetailResponse(totalMaintenanceActivities, lastMaintenanceDate);

                return ApiResponse<UserVehicleDetailResponse>.SuccessResponse(
                    response,
                    "Lấy thông tin xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user vehicle with ID: {VehicleId}", vehicleId);
                return ApiResponse<UserVehicleDetailResponse>.FailureResponse("Lỗi khi lấy thông tin xe");
            }
        }

        public async Task<ApiResponse<List<UserVehicleResponse>>> GetUserVehiclesAsync(Guid userId, PaginationRequest paginationRequest)
        {
            try
            {
                var query = _unitOfWork.UserVehicles.GetQueryWithFullDetails()
                    .Where(v => v.UserId == userId);

                var totalCount = await query.CountAsync();

                if (paginationRequest.IsDescending.HasValue)
                {
                    query = paginationRequest.IsDescending.Value
                        ? query.OrderByDescending(v => v.CreatedAt)
                        : query.OrderBy(v => v.CreatedAt);
                }

                var items = await query
                    .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                    .Take(paginationRequest.PageSize)
                    .ToListAsync();

                var vehicleResponses = items.Select(v => v.ToResponse()).ToList();

                return ApiResponse<UserVehicleResponse>.SuccessPagedResponse(
                    vehicleResponses,
                    totalCount,
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    "Lấy danh sách xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user vehicles for user: {UserId}", userId);
                return ApiResponse<List<UserVehicleResponse>>.FailureResponse("Lỗi khi lấy danh sách xe");
            }
        }

        public async Task<ApiResponse<VehicleStreakResponse>> GetVehicleStreakAsync(Guid userVehicleId)
        {
            var streak = await _unitOfWork.OdometerHistories.GetCurrentStreakAsync(userVehicleId);
            return ApiResponse<VehicleStreakResponse>.SuccessResponse(streak.ToStreakResponse(userVehicleId), "Lấy chuỗi xe thành công");
        }

        public async Task<ApiResponse<List<UserVehiclePartSummary>>> GetPartsByUserVehicleAsync(Guid userId, Guid userVehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<List<UserVehiclePartSummary>>.FailureResponse("Không tìm thấy xe");
                }

                var trackings = await _unitOfWork.VehiclePartTrackings.GetByUserVehicleIdAsync(userVehicleId);
                var summaries = trackings.Select(t => t.ToUserVehiclePartSummary()).ToList();

                return ApiResponse<List<UserVehiclePartSummary>>.SuccessResponse(
                    summaries,
                    "Lấy danh sách phụ tùng xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts for user vehicle {UserVehicleId}", userVehicleId);
                return ApiResponse<List<UserVehiclePartSummary>>.FailureResponse("Lỗi khi lấy danh sách phụ tùng xe");
            }
        }

        public async Task<ApiResponse<List<ReminderWithPartCategoryDto>>> GetRemindersAsync(Guid userId, Guid userVehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<List<ReminderWithPartCategoryDto>>.FailureResponse("Không tìm thấy xe");
                }

                var reminders = await _unitOfWork.MaintenanceReminders.GetByUserVehicleIdAsync(userVehicleId);
                var dtos = reminders.Select(r => r.ToReminderWithPartCategoryDto(vehicle.CurrentOdometer)).ToList();

                return ApiResponse<List<ReminderWithPartCategoryDto>>.SuccessResponse(
                    dtos,
                    "Lấy danh sách nhắc bảo trì thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reminders for user vehicle {UserVehicleId}", userVehicleId);
                return ApiResponse<List<ReminderWithPartCategoryDto>>.FailureResponse("Lỗi khi lấy danh sách nhắc bảo trì");
            }
        }

        public async Task<ApiResponse<List<OdometerHistoryItemDto>>> GetOdometerHistoryPagedAsync(Guid userId, Guid userVehicleId, OdometerHistoryQueryRequest query)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<List<OdometerHistoryItemDto>>.FailureResponse("Không tìm thấy xe");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting odometer history for user vehicle {UserVehicleId}", userVehicleId);
                return ApiResponse<List<OdometerHistoryItemDto>>.FailureResponse("Lỗi khi lấy lịch sử số km");
            }
        }

        public async Task<ApiResponse<UserVehicleResponse>> UpdateOdometerAsync(Guid userId, Guid vehicleId, UpdateOdometerRequest request)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Không tìm thấy xe");
                }

                if (request.CurrentOdometer < vehicle.CurrentOdometer)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Số km mới phải lớn hơn hoặc bằng số km hiện tại");
                }

                if (request.CurrentOdometer != vehicle.CurrentOdometer)
                {
                    var odometerHistory = vehicleId.ToOdometerHistory(request.CurrentOdometer, vehicle.CurrentOdometer);

                    await _unitOfWork.OdometerHistories.AddAsync(odometerHistory);

                    vehicle.UpdateOdometer(request.CurrentOdometer);
                    await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);

                    await SyncMaintenanceRemindersAsync(vehicleId, request.CurrentOdometer, userId);

                    await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);

                    _logger.LogInformation("Updated odometer for vehicle: {VehicleId} from {OldOdometer} to {NewOdometer} km",
                        vehicleId, vehicle.CurrentOdometer, request.CurrentOdometer);
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

        public async Task<ApiResponse<UserVehicleResponse>> UpdateUserVehicleAsync(Guid userId, Guid vehicleId, UserVehicleRequest request)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Không tìm thấy xe");
                }

                var vehicleVariant = await _unitOfWork.VehicleVariants
                    .GetByIdAsync(request.VehicleVariantId);

                if (vehicleVariant == null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Phiên bản xe không tồn tại");
                }

                var existingVehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.UserId == userId
                                    && v.LicensePlate == request.LicensePlate
                                    && v.Id != vehicleId
                                    );

                if (existingVehicle != null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Biển số xe đã tồn tại trong danh sách của bạn");
                }

                vehicle.UpdateEntity(request);
                await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);
                await _unitOfWork.SaveChangesAsync();

                // Load navigation properties
                var updatedVehicle = await _unitOfWork.UserVehicles.GetByIdWithFullDetailsAsync(vehicleId);

                _logger.LogInformation("Updated user vehicle with ID: {VehicleId} for user: {UserId}", vehicleId, userId);

                return ApiResponse<UserVehicleResponse>.SuccessResponse(
                    updatedVehicle!.ToResponse(),
                    "Cập nhật xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user vehicle with ID: {VehicleId}", vehicleId);
                return ApiResponse<UserVehicleResponse>.FailureResponse("Lỗi khi cập nhật xe");
            }
        }


        public async Task<ApiResponse<VehiclePartTrackingSummary>> ApplyTrackingConfigAsync(
            Guid userId, Guid vehicleId, ApplyTrackingConfigRequest request)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.Variant)
                        .ThenInclude(vv => vv.VehicleModel)
                    .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<VehiclePartTrackingSummary>.FailureResponse("Không tìm thấy xe");
                }

                // Find the PartCategory by code
                var partCategory = await _unitOfWork.PartCategories.AsQueryable()
                    .FirstOrDefaultAsync(pc => pc.Code == request.PartCategoryCode);

                if (partCategory == null)
                {
                    return ApiResponse<VehiclePartTrackingSummary>.FailureResponse(
                        $"Không tìm thấy linh kiện với mã '{request.PartCategoryCode}'");
                }


                // Find existing tracking record for this part
                var existingTracking = await _unitOfWork.VehiclePartTrackings.AsQueryable()
                    .Include(t => t.PartCategory)
                    .FirstOrDefaultAsync(t => t.UserVehicleId == vehicleId
                        && t.PartCategoryId == partCategory.Id);

                VehiclePartTracking tracking;

                if (existingTracking == null)
                {
                    tracking = vehicleId.ToPartTracking(partCategory.Id, request);

                    await _unitOfWork.VehiclePartTrackings.AddAsync(tracking);
                    await _unitOfWork.SaveChangesAsync();

                    tracking.PartCategory = partCategory;

                    _logger.LogInformation(
                        "Created new tracking for vehicle {VehicleId}, part {PartCode}",
                        vehicleId, request.PartCategoryCode);
                }
                else
                {
                    existingTracking.ApplyTrackingConfig(request);

                    await _unitOfWork.VehiclePartTrackings.UpdateAsync(existingTracking.Id, existingTracking);
                    await _unitOfWork.SaveChangesAsync();

                    tracking = existingTracking;

                    _logger.LogInformation(
                        "Updated existing tracking for vehicle {VehicleId}, part {PartCode}",
                        vehicleId, request.PartCategoryCode);
                }

                // Sync reminders (quay về Normal khi thay phụ tùng với Last*/PredictedNext* mới)
                await SyncMaintenanceRemindersAsync(vehicleId, vehicle.CurrentOdometer, userId);

                // Publish maintenance reminder notification if there are Critical reminders
                await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);

                // Return tracking summary with current vehicle odometer
                var response = tracking.ToSummary(vehicle.CurrentOdometer);

                return ApiResponse<VehiclePartTrackingSummary>.SuccessResponse(
                    response,
                    "Áp dụng cấu hình theo dõi thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error applying tracking config for vehicle {VehicleId}, part {PartCode}",
                    vehicleId, request.PartCategoryCode);
                return ApiResponse<VehiclePartTrackingSummary>.FailureResponse(
                    "Lỗi khi áp dụng cấu hình theo dõi");
            }
        }

        public async Task<ApiResponse<UserVehicleResponse>> CompleteOnboardingAsync(Guid userId, Guid vehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Không tìm thấy xe");
                }

                vehicle.NeedsOnboarding = false;
                await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);
                await _unitOfWork.SaveChangesAsync();

                var updatedVehicle = await _unitOfWork.UserVehicles.GetByIdWithFullDetailsAsync(vehicleId);

                _logger.LogInformation("Completed onboarding for vehicle: {VehicleId}", vehicleId);

                return ApiResponse<UserVehicleResponse>.SuccessResponse(
                    updatedVehicle!.ToResponse(),
                    "Hoàn thành onboarding thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing onboarding for vehicle: {VehicleId}", vehicleId);
                return ApiResponse<UserVehicleResponse>.FailureResponse("Lỗi khi hoàn thành onboarding");
            }
        }

        public async Task SyncMaintenanceRemindersForVehicleAsync(Guid vehicleId, int currentOdometer, Guid userId)
        {
            await SyncMaintenanceRemindersAsync(vehicleId, currentOdometer, userId);
            await _maintenanceReminderService.PublishMaintenanceReminderIfNeededAsync(vehicleId, userId);
        }

        private async Task InitializePartTrackingAsync(Guid userVehicleId)
        {
            var partCategories = await _unitOfWork.PartCategories.AsQueryable()
                .Where(pc => pc.DeletedAt == null)
                .ToListAsync();

            foreach (var partCategory in partCategories)
            {
                var tracking = userVehicleId.ToInitializePartTracking(partCategory.Id);

                await _unitOfWork.VehiclePartTrackings.AddAsync(tracking);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task SyncMaintenanceRemindersAsync(Guid vehicleId, int currentOdometer, Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var kmPerDay = await GetKmPerDayFromLast3MonthsAsync(vehicleId);

            if (kmPerDay is null)
            {
                var vehicle = await _unitOfWork.UserVehicles.FindOneAsync(v => v.Id == vehicleId);
                if (vehicle?.AverageKmPerDay is > 0)
                    kmPerDay = vehicle.AverageKmPerDay.Value;
            }

            var trackings = await _unitOfWork.VehiclePartTrackings.AsQueryable()
                .Where(t => t.UserVehicleId == vehicleId && t.IsDeclared &&
                    (t.PredictedNextOdometer != null || t.PredictedNextDate != null))
                .ToListAsync();

            foreach (var tracking in trackings)
            {
                var (percentageRemaining, targetDate) = ComputeReminderData(tracking, currentOdometer, today, kmPerDay);
                if (percentageRemaining is null)
                    continue;

                var targetOdometer = tracking.PredictedNextOdometer ?? currentOdometer;
                var level = GetLevelFromPercentage(percentageRemaining.Value);

                var existingReminders = await _unitOfWork.MaintenanceReminders.AsQueryable()
                    .Where(r => r.VehiclePartTrackingId == tracking.Id)
                    .OrderByDescending(r => r.Level)
                    .ToListAsync();

                var toKeep = existingReminders.FirstOrDefault();
                if (toKeep != null)
                {
                    toKeep.CurrentOdometer = currentOdometer;
                    toKeep.TargetOdometer = targetOdometer;
                    toKeep.TargetDate = targetDate;
                    toKeep.Level = level;
                    toKeep.PercentageRemaining = percentageRemaining.Value;
                    await _unitOfWork.MaintenanceReminders.UpdateAsync(toKeep.Id, toKeep);

                    foreach (var duplicate in existingReminders.Skip(1))
                    {
                        await _unitOfWork.MaintenanceReminders.DeleteAsync(duplicate.Id);
                    }
                }
                else
                {
                    var reminder = new MaintenanceReminder
                    {
                        VehiclePartTrackingId = tracking.Id,
                        CurrentOdometer = currentOdometer,
                        TargetOdometer = targetOdometer,
                        TargetDate = targetDate,
                        Level = level,
                        PercentageRemaining = percentageRemaining.Value,
                    };
                    await _unitOfWork.MaintenanceReminders.AddAsync(reminder);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<decimal?> GetKmPerDayFromLast3MonthsAsync(Guid vehicleId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var fromDate = today.AddMonths(-3);

            var history = await _unitOfWork.OdometerHistories.AsQueryable()
                .Where(h => h.UserVehicleId == vehicleId && h.RecordedDate >= fromDate)
                .OrderBy(h => h.RecordedDate)
                .Select(h => new { h.RecordedDate, h.OdometerValue })
                .ToListAsync();

            if (history.Count < 2)
                return null;

            var first = history.First();
            var last = history.Last();
            var totalKm = last.OdometerValue - first.OdometerValue;
            var totalDays = last.RecordedDate.DayNumber - first.RecordedDate.DayNumber;

            if (totalDays <= 0 || totalKm < 0)
                return null;

            return Math.Round((decimal)totalKm / totalDays, 2);
        }

        private static ReminderLevel GetLevelFromPercentage(decimal percentageRemaining)
        {
            if (percentageRemaining > 40) return ReminderLevel.Normal;
            if (percentageRemaining > 25) return ReminderLevel.Low;
            if (percentageRemaining > 15) return ReminderLevel.Medium;
            if (percentageRemaining > 5) return ReminderLevel.High;
            return ReminderLevel.Critical;
        }

        private static (decimal? PercentageRemaining, DateOnly? TargetDate) ComputeReminderData(
            VehiclePartTracking tracking,
            int currentOdometer,
            DateOnly today,
            decimal? kmPerDay)
        {
            decimal? percentageKm = null;
            decimal? percentageDate = null;
            DateOnly? targetDate = tracking.PredictedNextDate;

            if (tracking.PredictedNextOdometer.HasValue)
            {
                int intervalKm = tracking.LastReplacementOdometer.HasValue
                    ? tracking.PredictedNextOdometer.Value - tracking.LastReplacementOdometer.Value
                    : (tracking.CustomKmInterval ?? 1);
                if (intervalKm <= 0) intervalKm = 1;
                var remainingKm = tracking.PredictedNextOdometer.Value - currentOdometer;
                percentageKm = Math.Clamp(remainingKm * 100m / intervalKm, 0, 100);

                if (!tracking.PredictedNextDate.HasValue && kmPerDay.HasValue && kmPerDay > 0 && remainingKm > 0)
                {
                    var estimatedDaysRemaining = remainingKm / kmPerDay.Value;
                    var estimatedTargetDate = today.AddDays((int)Math.Ceiling(estimatedDaysRemaining));
                    targetDate = estimatedTargetDate;

                    var intervalDays = intervalKm / kmPerDay.Value;
                    if (intervalDays <= 0) intervalDays = 30;
                    percentageDate = Math.Clamp(estimatedDaysRemaining * 100m / intervalDays, 0, 100);
                }
            }

            if (tracking.PredictedNextDate.HasValue)
            {
                int intervalDays = tracking.LastReplacementDate.HasValue
                    ? tracking.PredictedNextDate.Value.DayNumber - tracking.LastReplacementDate.Value.DayNumber
                    : (tracking.CustomMonthsInterval ?? 1) * 30;
                if (intervalDays <= 0) intervalDays = 30;
                var remainingDays = tracking.PredictedNextDate.Value.DayNumber - today.DayNumber;
                percentageDate = Math.Clamp(remainingDays * 100m / intervalDays, 0, 100);
            }

            decimal? percentage = percentageKm.HasValue && percentageDate.HasValue
                ? Math.Min(percentageKm.Value, percentageDate.Value)
                : (percentageKm ?? percentageDate);

            return (percentage, targetDate);
        }
    }
}
