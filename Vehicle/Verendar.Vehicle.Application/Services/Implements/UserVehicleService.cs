using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class UserVehicleService(ILogger<UserVehicleService> logger, IUnitOfWork unitOfWork) : IUserVehicleService
    {
        private readonly ILogger<UserVehicleService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

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
                var vehicle = await _unitOfWork.UserVehicles.GetByIdAndUserIdWithoutPartTrackingsAsync(vehicleId, userId);

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

        public async Task<ApiResponse<List<UserVehiclePartSummary>>> GetDeclaredTrackingsByUserVehicleAsync(Guid userId, Guid userVehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == userVehicleId && v.UserId == userId);

                if (vehicle == null)
                {
                    return ApiResponse<List<UserVehiclePartSummary>>.FailureResponse("Không tìm thấy xe");
                }

                var trackings = await _unitOfWork.VehiclePartTrackings.GetDeclaredByUserVehicleIdAsync(userVehicleId);
                var summaries = trackings.Select(t => t.ToUserVehiclePartSummary()).ToList();

                return ApiResponse<List<UserVehiclePartSummary>>.SuccessResponse(
                    summaries,
                    "Lấy danh sách tracking đã khai báo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting declared trackings for user vehicle {UserVehicleId}", userVehicleId);
                return ApiResponse<List<UserVehiclePartSummary>>.FailureResponse("Lỗi khi lấy danh sách tracking đã khai báo");
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

                // Only create history if odometer actually changed
                if (request.CurrentOdometer != vehicle.CurrentOdometer)
                {
                    // Create odometer history record
                    var odometerHistory = vehicleId.ToOdometerHistory(request.CurrentOdometer);

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
                    tracking = vehicleId.ToInitializePartTracking(partCategory.Id);

                    await _unitOfWork.VehiclePartTrackings.AddAsync(tracking);
                    await _unitOfWork.SaveChangesAsync();

                    tracking.PartCategory = partCategory;

                    _logger.LogInformation(
                        "Created new tracking for vehicle {VehicleId}, part {PartCode}",
                        vehicleId, request.PartCategoryCode);
                }
                else
                {
                    existingTracking.LastReplacementOdometer = request.LastReplacementOdometer;
                    existingTracking.LastReplacementDate = request.LastReplacementDate;
                    existingTracking.PredictedNextOdometer = request.PredictedNextOdometer;
                    existingTracking.PredictedNextDate = request.PredictedNextDate;
                    existingTracking.IsDeclared = true;

                    await _unitOfWork.VehiclePartTrackings.UpdateAsync(existingTracking.Id, existingTracking);
                    await _unitOfWork.SaveChangesAsync();

                    tracking = existingTracking;

                    _logger.LogInformation(
                        "Updated existing tracking for vehicle {VehicleId}, part {PartCode}",
                        vehicleId, request.PartCategoryCode);
                }

                // Return tracking summary
                var response = tracking.ToSummary();

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
    }
}
