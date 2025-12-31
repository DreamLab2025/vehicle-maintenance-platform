using Microsoft.Extensions.Logging;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Mappings;
using VMP.Vehicle.Application.Services.Interfaces;
using VMP.Vehicle.Domain.Repositories.Interfaces;

namespace VMP.Vehicle.Application.Services.Implements
{
    public class VehicleTypeService : IVehicleTypeService
    {
        private readonly ILogger<VehicleTypeService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleTypeService(ILogger<VehicleTypeService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<TypeResponse>> CreateTypeAsync(TypeRequest request)
        {
            try
            {
                var existingType = await _unitOfWork.VehicleTypes
                    .FindOneAsync(t => t.Name == request.Name && t.DeletedAt == null);

                if (existingType != null)
                {
                    return ApiResponse<TypeResponse>.FailureResponse("Loại xe đã tồn tại");
                }

                var vehicleType = request.ToEntity();
                await _unitOfWork.VehicleTypes.AddAsync(vehicleType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created vehicle type with ID: {TypeId}", vehicleType.Id);

                return ApiResponse<TypeResponse>.SuccessResponse(
                    vehicleType.ToResponse(),
                    "Tạo loại xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle type");
                return ApiResponse<TypeResponse>.FailureResponse("Lỗi khi tạo loại xe");
            }
        }

        public async Task<ApiResponse<string>> DeleteTypeAsync(Guid id)
        {
            try
            {
                var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(id);

                if (vehicleType == null || vehicleType.DeletedAt != null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm thấy loại xe");
                }

                await _unitOfWork.VehicleTypes.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted vehicle type with ID: {TypeId}", id);

                return ApiResponse<string>.SuccessResponse(
                    "Deleted",
                    "Xóa loại xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle type with ID: {TypeId}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xóa loại xe");
            }
        }

        public async Task<ApiResponse<List<TypeResponse>>> GetAllTypesAsync(PaginationRequest paginationRequest)
        {
            try
            {
                var (items, totalCount) = await _unitOfWork.VehicleTypes.GetPagedAsync(
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    filter: x => x.DeletedAt == null,
                    orderBy: q => paginationRequest.IsDescending
                        ? q.OrderByDescending(t => t.CreatedAt)
                        : q.OrderBy(t => t.CreatedAt)
                );

                var typeResponses = items.Select(t => t.ToResponse()).ToList();

                return ApiResponse<TypeResponse>.SuccessPagedResponse(
                    typeResponses,
                    totalCount,
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    "Lấy danh sách loại xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vehicle types");
                return ApiResponse<List<TypeResponse>>.FailureResponse("Lỗi khi lấy danh sách loại xe");
            }
        }

        public async Task<ApiResponse<TypeResponse>> UpdateTypeAsync(Guid id, TypeRequest request)
        {
            try
            {
                var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(id);

                if (vehicleType == null || vehicleType.DeletedAt != null)
                {
                    return ApiResponse<TypeResponse>.FailureResponse("Không tìm thấy loại xe");
                }

                var existingType = await _unitOfWork.VehicleTypes
                    .FindOneAsync(t => t.Name == request.Name && t.Id != id && t.DeletedAt == null);

                if (existingType != null)
                {
                    return ApiResponse<TypeResponse>.FailureResponse("Tên loại xe đã tồn tại");
                }

                vehicleType.UpdateEntity(request);
                await _unitOfWork.VehicleTypes.UpdateAsync(id, vehicleType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated vehicle type with ID: {TypeId}", id);

                return ApiResponse<TypeResponse>.SuccessResponse(
                    vehicleType.ToResponse(),
                    "Cập nhật loại xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle type with ID: {TypeId}", id);
                return ApiResponse<TypeResponse>.FailureResponse("Lỗi khi cập nhật loại xe");
            }
        }
    }
}
