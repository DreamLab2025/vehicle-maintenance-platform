using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Databases.Base;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class UserVehicleService : IUserVehicleService
    {
        private readonly ILogger<UserVehicleService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public UserVehicleService(ILogger<UserVehicleService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<UserVehicleResponse>> CreateUserVehicleAsync(Guid userId, UserVehicleRequest request)
        {
            try
            {
                // Get vehicle variant with model info
                var vehicleVariant = await _unitOfWork.VehicleVariants.AsQueryable()
                    .Include(v => v.VehicleModel)
                    .FirstOrDefaultAsync(v => v.Id == request.VehicleVariantId && v.DeletedAt == null);

                if (vehicleVariant == null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Phiên bản xe không tồn tại");
                }

                var (isAllowed, message) = await _unitOfWork.UserVehicles
                    .CheckCanCreateVehicleAsync(userId);

                if (!isAllowed)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse(message);
                }

                var existingVehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.UserId == userId && v.LicensePlate == request.LicensePlate && v.DeletedAt == null);

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
                var initialOdometerHistory = new Domain.Entities.OdometerHistory
                {
                    UserVehicleId = userVehicle.Id,
                    OdometerValue = request.CurrentOdometer,
                    RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Source = Domain.Enums.OdometerSource.ManualInput
                };

                await _unitOfWork.OdometerHistories.AddAsync(initialOdometerHistory);
                await _unitOfWork.SaveChangesAsync();

                await InitializePartTrackingFromDefaultScheduleAsync(userVehicle.Id, vehicleVariant.VehicleModelId);

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
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId && v.DeletedAt == null);

                if (vehicle == null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm thấy xe");
                }

                await _unitOfWork.UserVehicles.DeleteAsync(vehicleId);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted user vehicle with ID: {VehicleId} for user: {UserId}", vehicleId, userId);

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
                var vehicle = await _unitOfWork.UserVehicles.GetByIdAndUserIdWithFullDetailsAsync(vehicleId, userId);

                if (vehicle == null)
                {
                    return ApiResponse<UserVehicleDetailResponse>.FailureResponse("Không tìm thấy xe");
                }

                // Get maintenance activities count (would need MaintenanceActivity repository)
                // For now, set to 0
                var totalMaintenanceActivities = 0;
                DateTime? lastMaintenanceDate = null;

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

        public async Task<ApiResponse<UserVehicleResponse>> UpdateOdometerAsync(Guid userId, Guid vehicleId, UpdateOdometerRequest request)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId && v.DeletedAt == null);

                if (vehicle == null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Không tìm thấy xe");
                }

                if (request.CurrentOdometer < vehicle.CurrentOdometer)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Số km mới phải lớn hơn hoặc bằng số km hiện tại");
                }

                // Only create history if odometer actually changed
                if (request.CurrentOdometer != vehicle.CurrentOdometer)
                {
                    // Create odometer history record
                    var odometerHistory = new Domain.Entities.OdometerHistory
                    {
                        UserVehicleId = vehicleId,
                        OdometerValue = request.CurrentOdometer,
                        RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        Source = Domain.Enums.OdometerSource.ManualInput
                    };

                    await _unitOfWork.OdometerHistories.AddAsync(odometerHistory);

                    // Update vehicle odometer
                    vehicle.UpdateOdometer(request.CurrentOdometer);
                    await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Updated odometer for vehicle: {VehicleId} from {OldOdometer} to {NewOdometer} km",
                        vehicleId, vehicle.CurrentOdometer, request.CurrentOdometer);
                }

                // Load navigation properties
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
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId && v.DeletedAt == null);

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
                                    && v.DeletedAt == null);

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


        private async Task InitializePartTrackingFromDefaultScheduleAsync(Guid userVehicleId, Guid vehicleModelId)
        {
            try
            {
                var defaultSchedules = await _unitOfWork.DefaultMaintenanceSchedules.AsQueryable()
                    .Include(s => s.PartCategory)
                    .Where(s => s.VehicleModelId == vehicleModelId && s.DeletedAt == null && s.Status == EntityStatus.Active)
                    .ToListAsync();

                if (!defaultSchedules.Any())
                {
                    _logger.LogInformation("No default maintenance schedules found for model: {VehicleModelId}", vehicleModelId);
                    return;
                }

                foreach (var schedule in defaultSchedules)
                {
                    var partTracking = new Domain.Entities.VehiclePartTracking
                    {
                        UserVehicleId = userVehicleId,
                        PartCategoryId = schedule.PartCategoryId,
                        InstanceIdentifier = null,
                        CustomKmInterval = schedule.KmInterval,
                        CustomMonthsInterval = schedule.MonthsInterval,
                        Status = EntityStatus.Active
                    };

                    await _unitOfWork.VehiclePartTrackings.AddAsync(partTracking);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Initialized {Count} part tracking records for vehicle: {UserVehicleId}",
                    defaultSchedules.Count, userVehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing part tracking for vehicle: {UserVehicleId}", userVehicleId);
            }
        }
    }
}
