using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class TypeService(ILogger<TypeService> logger, IUnitOfWork unitOfWork) : ITypeService
    {
        private readonly ILogger<TypeService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<TypeResponse>> CreateTypeAsync(TypeRequest request)
        {
            try
            {
                var existingType = await _unitOfWork.Types
                    .FindOneAsync(t => t.Name == request.Name && t.DeletedAt == null);

                if (existingType != null)
                {
                    return ApiResponse<TypeResponse>.ConflictResponse("Loại xe đã tồn tại");
                }

                var vehicleType = request.ToEntity();
                await _unitOfWork.Types.AddAsync(vehicleType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created vehicle type with ID: {TypeId}", vehicleType.Id);

                return ApiResponse<TypeResponse>.CreatedResponse(
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
                var vehicleType = await _unitOfWork.Types.GetByIdAsync(id);

                if (vehicleType == null || vehicleType.DeletedAt != null)
                {
                    return ApiResponse<string>.NotFoundResponse("Không tìm thấy loại xe");
                }

                await _unitOfWork.Types.DeleteAsync(id);
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
                paginationRequest.Normalize();
                var (items, totalCount) = await _unitOfWork.Types.GetPagedAsync(
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    filter: x => x.DeletedAt == null,
                    orderBy: paginationRequest.IsDescending.HasValue
                        ? (paginationRequest.IsDescending.Value
                            ? q => q.OrderByDescending(t => t.CreatedAt)
                            : q => q.OrderBy(t => t.CreatedAt))
                        : null
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
                var vehicleType = await _unitOfWork.Types.GetByIdAsync(id);

                if (vehicleType == null || vehicleType.DeletedAt != null)
                {
                    return ApiResponse<TypeResponse>.NotFoundResponse("Không tìm thấy loại xe");
                }

                var existingType = await _unitOfWork.Types
                    .FindOneAsync(t => t.Name == request.Name && t.Id != id && t.DeletedAt == null);

                if (existingType != null)
                {
                    return ApiResponse<TypeResponse>.ConflictResponse("Tên loại xe đã tồn tại");
                }

                vehicleType.UpdateEntity(request);
                await _unitOfWork.Types.UpdateAsync(id, vehicleType);
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
