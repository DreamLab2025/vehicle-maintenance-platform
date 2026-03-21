using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class UserVehicleService(
        ILogger<UserVehicleService> logger,
        IUnitOfWork unitOfWork,
        IPartTrackingService vehiclePartTrackingService) : IUserVehicleService
    {
        private readonly ILogger<UserVehicleService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPartTrackingService _vehiclePartTrackingService = vehiclePartTrackingService;

        public async Task<ApiResponse<IsAllowedToCreateVehicleResponse>> IsAllowedToCreateVehicleAsync(Guid userId)
        {
            try
            {
                var (isAllowed, message) = await _unitOfWork.UserVehicles.CheckCanCreateVehicleAsync(userId);
                return ApiResponse<IsAllowedToCreateVehicleResponse>.SuccessResponse(
                    isAllowed.ToIsAllowedToCreateVehicleResponse(message),
                    "Kiểm tra quyền tạo xe thành công");
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
                var vehicleVariant = await _unitOfWork.Variants.AsQueryable()
                    .Include(v => v.VehicleModel)
                    .FirstOrDefaultAsync(v => v.Id == request.VehicleVariantId);

                if (vehicleVariant == null)
                    return ApiResponse<UserVehicleResponse>.NotFoundResponse("Phiên bản xe không tồn tại");

                var existingVehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.UserId == userId && v.LicensePlate == request.LicensePlate);

                if (existingVehicle != null)
                {
                    _logger.LogWarning("Attempt to create duplicate vehicle with license plate: {LicensePlate} for user: {UserId}",
                        request.LicensePlate, userId);
                    return ApiResponse<UserVehicleResponse>.ConflictResponse("Biển số xe đã tồn tại trong danh sách của bạn");
                }

                var userVehicle = request.ToEntity(userId);
                await _unitOfWork.UserVehicles.AddAsync(userVehicle);
                await _unitOfWork.SaveChangesAsync();

                var initialOdometerHistory = userVehicle.Id.ToOdometerHistory(request.CurrentOdometer);
                await _unitOfWork.OdometerHistories.AddAsync(initialOdometerHistory);
                await _unitOfWork.SaveChangesAsync();

                await _vehiclePartTrackingService.InitializeForVehicleAsync(userVehicle.Id, vehicleVariant.VehicleModelId);

                var createdVehicle = await _unitOfWork.UserVehicles.GetByIdWithFullDetailsAsync(userVehicle.Id);

                _logger.LogInformation("Created user vehicle with ID: {VehicleId} for user: {UserId}", userVehicle.Id, userId);

                return ApiResponse<UserVehicleResponse>.CreatedResponse(
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
                    return ApiResponse<string>.NotFoundResponse("Không tìm thấy xe");

                await _unitOfWork.BeginTransactionAsync();

                var deletedAt = DateTime.UtcNow;

                var reminders = await _unitOfWork.MaintenanceReminders.GetByUserVehicleIdAsync(vehicleId);
                foreach (var r in reminders)
                {
                    r.DeletedAt = deletedAt;
                    r.DeletedBy = userId;
                }

                var trackings = await _unitOfWork.PartTrackings.GetByUserVehicleIdAsync(vehicleId);
                foreach (var t in trackings)
                {
                    t.DeletedAt = deletedAt;
                    t.DeletedBy = userId;
                }

                var odometerHistories = await _unitOfWork.OdometerHistories.AsQueryable()
                    .Where(x => x.UserVehicleId == vehicleId)
                    .ToListAsync();
                foreach (var h in odometerHistories)
                {
                    h.DeletedAt = deletedAt;
                    h.DeletedBy = userId;
                }

                var records = await _unitOfWork.MaintenanceRecords.GetByUserVehicleIdAsync(vehicleId);
                foreach (var record in records)
                {
                    var items = await _unitOfWork.MaintenanceRecordItems.GetByMaintenanceRecordIdAsync(record.Id);
                    foreach (var item in items)
                    {
                        item.DeletedAt = deletedAt;
                        item.DeletedBy = userId;
                    }
                    record.DeletedAt = deletedAt;
                    record.DeletedBy = userId;
                }

                vehicle.DeletedAt = deletedAt;
                vehicle.DeletedBy = userId;

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Soft deleted user vehicle with ID: {VehicleId} for user: {UserId} (cascade)", vehicleId, userId);

                return ApiResponse<string>.SuccessResponse("Deleted", "Xóa xe thành công");
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
                    return ApiResponse<UserVehicleDetailResponse>.NotFoundResponse("Không tìm thấy xe");

                var recordsQuery = _unitOfWork.MaintenanceRecords.AsQueryable()
                    .Where(r => r.UserVehicleId == vehicleId);
                var totalMaintenanceActivities = await recordsQuery.CountAsync();
                DateTime? lastMaintenanceDate = null;
                if (totalMaintenanceActivities > 0)
                {
                    var lastServiceDate = await recordsQuery.MaxAsync(r => r.ServiceDate);
                    lastMaintenanceDate = lastServiceDate.ToDateTime(TimeOnly.MinValue);
                }

                return ApiResponse<UserVehicleDetailResponse>.SuccessResponse(
                    vehicle.ToDetailResponse(totalMaintenanceActivities, lastMaintenanceDate),
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
                paginationRequest.Normalize();
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

                return ApiResponse<UserVehicleResponse>.SuccessPagedResponse(
                    items.Select(v => v.ToResponse()).ToList(),
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

        public async Task<ApiResponse<UserVehicleResponse>> UpdateUserVehicleAsync(Guid userId, Guid vehicleId, UserVehicleRequest request)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<UserVehicleResponse>.NotFoundResponse("Không tìm thấy xe");

                var vehicleVariant = await _unitOfWork.Variants.GetByIdAsync(request.VehicleVariantId);

                if (vehicleVariant == null)
                    return ApiResponse<UserVehicleResponse>.NotFoundResponse("Phiên bản xe không tồn tại");

                var existingVehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.UserId == userId && v.LicensePlate == request.LicensePlate && v.Id != vehicleId);

                if (existingVehicle != null)
                    return ApiResponse<UserVehicleResponse>.ConflictResponse("Biển số xe đã tồn tại trong danh sách của bạn");

                vehicle.UpdateEntity(request);
                await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);
                await _unitOfWork.SaveChangesAsync();

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

        public async Task<ApiResponse<UserVehicleResponse>> CompleteOnboardingAsync(Guid userId, Guid vehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

                if (vehicle == null)
                    return ApiResponse<UserVehicleResponse>.NotFoundResponse("Không tìm thấy xe");

                if (!vehicle.NeedsOnboarding)
                {
                    return ApiResponse<UserVehicleResponse>.SuccessResponse(
                        (await _unitOfWork.UserVehicles.GetByIdWithFullDetailsAsync(vehicleId))!.ToResponse(),
                        "Onboarding đã hoàn thành trước đó");
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
    }
}
