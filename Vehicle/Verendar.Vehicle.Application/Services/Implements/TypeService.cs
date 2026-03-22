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
            var existingType = await _unitOfWork.Types
                .FindOneAsync(t => t.Name == request.Name && t.DeletedAt == null);

            if (existingType != null)
            {
                _logger.LogWarning("CreateType: duplicate name {TypeName}", request.Name);
                return ApiResponse<TypeResponse>.ConflictResponse("Loại xe đã tồn tại");
            }

            var vehicleType = request.ToEntity();
            await _unitOfWork.Types.AddAsync(vehicleType);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<TypeResponse>.CreatedResponse(
                vehicleType.ToResponse(),
                "Tạo loại xe thành công");
        }

        public async Task<ApiResponse<string>> DeleteTypeAsync(Guid id)
        {
            var vehicleType = await _unitOfWork.Types.GetByIdAsync(id);

            if (vehicleType == null || vehicleType.DeletedAt != null)
            {
                _logger.LogWarning("DeleteType: not found or deleted {TypeId}", id);
                return ApiResponse<string>.NotFoundResponse("Không tìm thấy loại xe");
            }

            await _unitOfWork.Types.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse(
                "Deleted",
                "Xóa loại xe thành công");
        }

        public async Task<ApiResponse<List<TypeSummary>>> GetAllTypesAsync(PaginationRequest paginationRequest)
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

            var typeSummaries = items.Select(t => t.ToSummary()).ToList();

            return ApiResponse<List<TypeSummary>>.SuccessPagedResponse(
                typeSummaries,
                totalCount,
                paginationRequest.PageNumber,
                paginationRequest.PageSize,
                "Lấy danh sách loại xe thành công");
        }

        public async Task<ApiResponse<TypeResponse>> GetTypeByIdAsync(Guid id)
        {
            var vehicleType = await _unitOfWork.Types.GetByIdAsync(id);
            if (vehicleType == null || vehicleType.DeletedAt != null)
            {
                _logger.LogWarning("GetTypeById: not found {TypeId}", id);
                return ApiResponse<TypeResponse>.NotFoundResponse("Không tìm thấy loại xe");
            }

            return ApiResponse<TypeResponse>.SuccessResponse(
                vehicleType.ToResponse(),
                "Lấy thông tin loại xe thành công");
        }

        public async Task<ApiResponse<TypeResponse>> UpdateTypeAsync(Guid id, TypeRequest request)
        {
            var vehicleType = await _unitOfWork.Types.GetByIdAsync(id);

            if (vehicleType == null || vehicleType.DeletedAt != null)
            {
                _logger.LogWarning("UpdateType: not found {TypeId}", id);
                return ApiResponse<TypeResponse>.NotFoundResponse("Không tìm thấy loại xe");
            }

            var existingType = await _unitOfWork.Types
                .FindOneAsync(t => t.Name == request.Name && t.Id != id && t.DeletedAt == null);

            if (existingType != null)
            {
                _logger.LogWarning("UpdateType: name conflict {TypeName} for {TypeId}", request.Name, id);
                return ApiResponse<TypeResponse>.ConflictResponse("Tên loại xe đã tồn tại");
            }

            vehicleType.UpdateEntity(request);
            await _unitOfWork.Types.UpdateAsync(id, vehicleType);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<TypeResponse>.SuccessResponse(
                vehicleType.ToResponse(),
                "Cập nhật loại xe thành công");
        }
    }
}
