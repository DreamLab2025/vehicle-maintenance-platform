using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

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
            var (isAllowed, message) = await _unitOfWork.UserVehicles.CheckCanCreateVehicleAsync(userId);
            return ApiResponse<IsAllowedToCreateVehicleResponse>.SuccessResponse(
                isAllowed.ToIsAllowedToCreateVehicleResponse(message),
                "Kiểm tra quyền tạo xe thành công");
        }

        public async Task<ApiResponse<UserVehicleResponse>> CreateUserVehicleAsync(Guid userId, UserVehicleRequest request)
        {
            var vehicleVariant = await _unitOfWork.Variants.GetByIdWithVehicleModelAsync(request.VehicleVariantId);

            if (vehicleVariant == null)
            {
                _logger.LogWarning("CreateUserVehicle: variant not found {VehicleVariantId} user {UserId}", request.VehicleVariantId, userId);
                return ApiResponse<UserVehicleResponse>.NotFoundResponse("Phiên bản xe không tồn tại");
            }

            var existingVehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.UserId == userId && v.LicensePlate == request.LicensePlate);

            if (existingVehicle != null)
            {
                _logger.LogWarning("CreateUserVehicle: duplicate license plate {LicensePlate} user {UserId}", request.LicensePlate, userId);
                return ApiResponse<UserVehicleResponse>.ConflictResponse("Biển số xe đã tồn tại trong danh sách của bạn");
            }

            var userVehicle = request.ToEntity(userId);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.UserVehicles.AddAsync(userVehicle);

                var initialOdometerHistory = userVehicle.Id.ToOdometerHistory(request.CurrentOdometer);
                await _unitOfWork.OdometerHistories.AddAsync(initialOdometerHistory);

                await _vehiclePartTrackingService.InitializeForVehicleAsync(userVehicle.Id, vehicleVariant.VehicleModelId);
            });

            var createdVehicle = await _unitOfWork.UserVehicles.GetByIdWithFullDetailsAsync(userVehicle.Id);

            return ApiResponse<UserVehicleResponse>.CreatedResponse(
                createdVehicle!.ToResponse(),
                "Thêm xe thành công");
        }

        public async Task<ApiResponse<string>> DeleteUserVehicleAsync(Guid userId, Guid vehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("DeleteUserVehicle: not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<string>.NotFoundResponse("Không tìm thấy xe");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
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

                var odometerHistories = await _unitOfWork.OdometerHistories.GetAllByUserVehicleIdAsync(vehicleId);
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
            });

            return ApiResponse<string>.SuccessResponse("Deleted", "Xóa xe thành công");
        }

        public async Task<ApiResponse<UserVehicleDetailResponse>> GetUserVehicleByIdAsync(Guid userId, Guid vehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles.GetByIdAndUserIdWithoutPartTrackingsAsync(vehicleId, userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetUserVehicleById: not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<UserVehicleDetailResponse>.NotFoundResponse("Không tìm thấy xe");
            }

            var (totalMaintenanceActivities, lastServiceDate) =
                await _unitOfWork.MaintenanceRecords.GetActivitySummaryByUserVehicleIdAsync(vehicleId);
            DateTime? lastMaintenanceDate = lastServiceDate.HasValue
                ? lastServiceDate.Value.ToDateTime(TimeOnly.MinValue)
                : null;

            return ApiResponse<UserVehicleDetailResponse>.SuccessResponse(
                vehicle.ToDetailResponse(totalMaintenanceActivities, lastMaintenanceDate),
                "Lấy thông tin xe thành công");
        }

        public async Task<ApiResponse<List<UserVehicleSummaryDto>>> GetUserVehiclesAsync(Guid userId, PaginationRequest paginationRequest)
        {
            paginationRequest.Normalize();
            var query = _unitOfWork.UserVehicles.GetQueryWithoutPartTrackings()
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

            return ApiResponse<UserVehicleSummaryDto>.SuccessPagedResponse(
                items.Select(v => v.ToSummaryDto()).ToList(),
                totalCount,
                paginationRequest.PageNumber,
                paginationRequest.PageSize,
                "Lấy danh sách xe thành công");
        }

        public async Task<ApiResponse<UserVehicleResponse>> UpdateUserVehicleAsync(Guid userId, Guid vehicleId, UserVehicleRequest request)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("UpdateUserVehicle: not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<UserVehicleResponse>.NotFoundResponse("Không tìm thấy xe");
            }

            var vehicleVariant = await _unitOfWork.Variants.GetByIdAsync(request.VehicleVariantId);

            if (vehicleVariant == null)
            {
                _logger.LogWarning("UpdateUserVehicle: variant not found {VehicleVariantId}", request.VehicleVariantId);
                return ApiResponse<UserVehicleResponse>.NotFoundResponse("Phiên bản xe không tồn tại");
            }

            var existingVehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.UserId == userId && v.LicensePlate == request.LicensePlate && v.Id != vehicleId);

            if (existingVehicle != null)
            {
                _logger.LogWarning("UpdateUserVehicle: license conflict {LicensePlate} user {UserId}", request.LicensePlate, userId);
                return ApiResponse<UserVehicleResponse>.ConflictResponse("Biển số xe đã tồn tại trong danh sách của bạn");
            }

            vehicle.UpdateFromRequest(request);
            await _unitOfWork.UserVehicles.UpdateAsync(vehicleId, vehicle);
            await _unitOfWork.SaveChangesAsync();

            var updatedVehicle = await _unitOfWork.UserVehicles.GetByIdWithFullDetailsAsync(vehicleId);

            return ApiResponse<UserVehicleResponse>.SuccessResponse(
                updatedVehicle!.ToResponse(),
                "Cập nhật xe thành công");
        }

        public async Task<ApiResponse<VehicleHealthScoreResponse>> GetHealthScoreAsync(Guid userId, Guid vehicleId)
        {
            var vehicle = await _unitOfWork.UserVehicles
                .FindOneAsync(v => v.Id == vehicleId && v.UserId == userId);

            if (vehicle == null)
            {
                _logger.LogWarning("GetHealthScore: vehicle not found {VehicleId} user {UserId}", vehicleId, userId);
                return ApiResponse<VehicleHealthScoreResponse>.NotFoundResponse("Không tìm thấy xe");
            }

            var declaredTrackings = await _unitOfWork.PartTrackings
                .GetDeclaredByUserVehicleIdAsync(vehicleId);

            return ApiResponse<VehicleHealthScoreResponse>.SuccessResponse(
                declaredTrackings.ToHealthScoreResponse(vehicleId),
                "Tính điểm sức khỏe xe thành công");
        }

    }
}
