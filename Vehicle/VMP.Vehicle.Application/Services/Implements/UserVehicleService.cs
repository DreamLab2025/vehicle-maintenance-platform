using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Mappings;
using VMP.Vehicle.Application.Services.Interfaces;
using VMP.Vehicle.Domain.Repositories.Interfaces;

namespace VMP.Vehicle.Application.Services.Implements
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
                var vehicleModel = await _unitOfWork.VehicleModels.GetByIdAsync(request.VehicleModelId);
                if (vehicleModel == null || vehicleModel.DeletedAt != null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Mẫu xe không tồn tại");
                }

                var existingVehicle = await _unitOfWork.UserVehicles
                    .FindOneAsync(v => v.UserId == userId && v.LicensePlate == request.LicensePlate && v.DeletedAt == null);

                if (existingVehicle != null)
                {
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
                    RecordedAt = DateTime.UtcNow,
                    Source = Domain.Entities.MaintenanceSource.ManualInput
                };
                
                await _unitOfWork.OdometerHistories.AddAsync(initialOdometerHistory);
                await _unitOfWork.SaveChangesAsync();

                var createdVehicle = await _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Brand)
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Type)
                    .FirstOrDefaultAsync(v => v.Id == userVehicle.Id);

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
                var vehicle = await _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Brand)
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Type)
                    .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId && v.DeletedAt == null);

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
                var query = _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Brand)
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Type)
                    .Where(v => v.UserId == userId && v.DeletedAt == null);

                var totalCount = await query.CountAsync();

                if (paginationRequest.IsDescending)
                {
                    query = query.OrderByDescending(v => v.CreatedAt);
                }
                else
                {
                    query = query.OrderBy(v => v.CreatedAt);
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
                        RecordedAt = DateTime.UtcNow,
                        Source = Domain.Entities.MaintenanceSource.ManualInput
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
                var updatedVehicle = await _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Brand)
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Type)
                    .FirstOrDefaultAsync(v => v.Id == vehicleId);

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

                // Validate vehicle model exists
                var vehicleModel = await _unitOfWork.VehicleModels.GetByIdAsync(request.VehicleModelId);
                if (vehicleModel == null || vehicleModel.DeletedAt != null)
                {
                    return ApiResponse<UserVehicleResponse>.FailureResponse("Mẫu xe không tồn tại");
                }

                // Check duplicate license plate (excluding current vehicle)
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
                var updatedVehicle = await _unitOfWork.UserVehicles.AsQueryable()
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Brand)
                    .Include(v => v.VehicleModel)
                        .ThenInclude(m => m.Type)
                    .FirstOrDefaultAsync(v => v.Id == vehicleId);

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
    }
}
