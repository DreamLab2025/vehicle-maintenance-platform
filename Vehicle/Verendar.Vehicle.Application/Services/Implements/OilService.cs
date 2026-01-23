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
    public class OilService : IOilService
    {
        private readonly ILogger<OilService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public OilService(ILogger<OilService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<OilResponse>> CreateOilAsync(OilRequest request)
        {
            try
            {
                // Validate VehiclePart exists and belongs to OIL category
                var vehiclePart = await _unitOfWork.VehicleParts.GetByIdAsync(request.VehiclePartId);
                if (vehiclePart == null || vehiclePart.DeletedAt != null)
                {
                    return ApiResponse<OilResponse>.FailureResponse("Không tìm thấy phụ tùng");
                }

                var oilCategory = await _unitOfWork.VehiclePartCategories
                    .GetByCodeAsync(VehiclePartCategoryCodes.Oil);

                if (oilCategory == null || vehiclePart.CategoryId != oilCategory.Id)
                {
                    return ApiResponse<OilResponse>.FailureResponse("Phụ tùng phải thuộc category Dầu nhớt");
                }

                // Check if Oil already exists for this VehiclePart
                var existingOil = await _unitOfWork.Oils.GetByVehiclePartIdAsync(request.VehiclePartId);
                if (existingOil != null && existingOil.DeletedAt == null)
                {
                    return ApiResponse<OilResponse>.FailureResponse("Đã tồn tại thông tin chi tiết nhớt cho phụ tùng này");
                }

                var oil = request.ToEntity();
                await _unitOfWork.Oils.AddAsync(oil);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                var createdOil = await _unitOfWork.Oils.GetByVehiclePartIdAsync(request.VehiclePartId);

                _logger.LogInformation("Created oil for VehiclePart {VehiclePartId} with ViscosityGrade {ViscosityGrade}",
                    request.VehiclePartId, request.ViscosityGrade);

                return ApiResponse<OilResponse>.SuccessResponse(
                    createdOil!.ToResponse(),
                    "Tạo thông tin nhớt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating oil for VehiclePart {VehiclePartId}", request.VehiclePartId);
                return ApiResponse<OilResponse>.FailureResponse("Lỗi khi tạo thông tin nhớt");
            }
        }

        public async Task<ApiResponse<string>> DeleteOilAsync(Guid id)
        {
            try
            {
                var oil = await _unitOfWork.Oils.GetByIdAsync(id);
                if (oil == null || oil.DeletedAt != null)
                {
                    return ApiResponse<string>.FailureResponse("Không tìm thấy thông tin nhớt");
                }

                await _unitOfWork.Oils.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted oil with ID: {OilId}", id);

                return ApiResponse<string>.SuccessResponse(
                    "Deleted",
                    "Xóa thông tin nhớt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting oil with ID: {OilId}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xóa thông tin nhớt");
            }
        }

        public async Task<ApiResponse<List<OilResponse>>> GetAllOilsAsync(PaginationRequest paginationRequest)
        {
            try
            {
                var baseQuery = _unitOfWork.Oils.AsQueryable()
                    .Where(o => o.DeletedAt == null)
                    .Include(o => o.VehiclePart)
                        .ThenInclude(p => p.Category);

                var totalCount = await baseQuery.CountAsync();

                IQueryable<Oil> orderedQuery;
                if (paginationRequest.IsDescending.HasValue)
                {
                    orderedQuery = paginationRequest.IsDescending.Value
                        ? baseQuery.OrderByDescending(o => o.CreatedAt)
                        : baseQuery.OrderBy(o => o.CreatedAt);
                }
                else
                {
                    orderedQuery = baseQuery;
                }

                var items = await orderedQuery
                    .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                    .Take(paginationRequest.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var oilResponses = items.Select(o => o.ToResponse()).ToList();

                return ApiResponse<List<OilResponse>>.SuccessPagedResponse(
                    oilResponses,
                    totalCount,
                    paginationRequest.PageNumber,
                    paginationRequest.PageSize,
                    "Lấy danh sách nhớt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all oils");
                return ApiResponse<List<OilResponse>>.FailureResponse("Lỗi khi lấy danh sách nhớt");
            }
        }

        public async Task<ApiResponse<OilResponse>> GetOilByIdAsync(Guid id)
        {
            try
            {
                var oil = await _unitOfWork.Oils.GetByIdAsync(id);
                if (oil == null || oil.DeletedAt != null)
                {
                    return ApiResponse<OilResponse>.FailureResponse("Không tìm thấy thông tin nhớt");
                }

                var oilWithDetails = await _unitOfWork.Oils.GetByVehiclePartIdAsync(oil.VehiclePartId);
                if (oilWithDetails == null)
                {
                    return ApiResponse<OilResponse>.FailureResponse("Không tìm thấy thông tin nhớt");
                }

                return ApiResponse<OilResponse>.SuccessResponse(
                    oilWithDetails.ToResponse(),
                    "Lấy thông tin nhớt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oil by ID: {OilId}", id);
                return ApiResponse<OilResponse>.FailureResponse("Lỗi khi lấy thông tin nhớt");
            }
        }

        public async Task<ApiResponse<OilResponse>> GetOilByVehiclePartIdAsync(Guid vehiclePartId)
        {
            try
            {
                var oil = await _unitOfWork.Oils.GetByVehiclePartIdAsync(vehiclePartId);
                if (oil == null)
                {
                    return ApiResponse<OilResponse>.FailureResponse("Không tìm thấy thông tin nhớt cho phụ tùng này");
                }

                return ApiResponse<OilResponse>.SuccessResponse(
                    oil.ToResponse(),
                    "Lấy thông tin nhớt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oil by VehiclePartId: {VehiclePartId}", vehiclePartId);
                return ApiResponse<OilResponse>.FailureResponse("Lỗi khi lấy thông tin nhớt");
            }
        }

        public async Task<ApiResponse<List<OilResponse>>> GetOilsByVehicleUsageAsync(OilVehicleUsage vehicleUsage)
        {
            try
            {
                var oils = await _unitOfWork.Oils.GetByVehicleUsageAsync(vehicleUsage);
                var oilResponses = oils.Select(o => o.ToResponse()).ToList();

                return ApiResponse<List<OilResponse>>.SuccessResponse(
                    oilResponses,
                    $"Lấy danh sách {oilResponses.Count} nhớt phù hợp với loại xe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oils by vehicle usage: {VehicleUsage}", vehicleUsage);
                return ApiResponse<List<OilResponse>>.FailureResponse("Lỗi khi lấy danh sách nhớt theo loại xe");
            }
        }

        public async Task<ApiResponse<List<OilResponse>>> GetOilsByViscosityGradeAsync(string viscosityGrade)
        {
            try
            {
                var oils = await _unitOfWork.Oils.GetByViscosityGradeAsync(viscosityGrade);
                var oilResponses = oils.Select(o => o.ToResponse()).ToList();

                return ApiResponse<List<OilResponse>>.SuccessResponse(
                    oilResponses,
                    $"Lấy danh sách {oilResponses.Count} nhớt với cấp độ {viscosityGrade} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oils by viscosity grade: {ViscosityGrade}", viscosityGrade);
                return ApiResponse<List<OilResponse>>.FailureResponse("Lỗi khi lấy danh sách nhớt theo cấp độ");
            }
        }

        public async Task<ApiResponse<OilResponse>> UpdateOilAsync(Guid id, OilRequest request)
        {
            try
            {
                var oil = await _unitOfWork.Oils.GetByIdAsync(id);
                if (oil == null || oil.DeletedAt != null)
                {
                    return ApiResponse<OilResponse>.FailureResponse("Không tìm thấy thông tin nhớt");
                }

                // Validate VehiclePart if changed
                if (oil.VehiclePartId != request.VehiclePartId)
                {
                    var vehiclePart = await _unitOfWork.VehicleParts.GetByIdAsync(request.VehiclePartId);
                    if (vehiclePart == null || vehiclePart.DeletedAt != null)
                    {
                        return ApiResponse<OilResponse>.FailureResponse("Không tìm thấy phụ tùng");
                    }

                    var oilCategory = await _unitOfWork.VehiclePartCategories
                        .GetByCodeAsync(VehiclePartCategoryCodes.Oil);

                    if (oilCategory == null || vehiclePart.CategoryId != oilCategory.Id)
                    {
                        return ApiResponse<OilResponse>.FailureResponse("Phụ tùng phải thuộc category Dầu nhớt");
                    }

                    // Check if another Oil exists for new VehiclePartId
                    var existingOil = await _unitOfWork.Oils.GetByVehiclePartIdAsync(request.VehiclePartId);
                    if (existingOil != null && existingOil.DeletedAt == null && existingOil.Id != id)
                    {
                        return ApiResponse<OilResponse>.FailureResponse("Đã tồn tại thông tin nhớt cho phụ tùng này");
                    }
                }

                oil.UpdateEntity(request);
                await _unitOfWork.Oils.UpdateAsync(id, oil);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                var updatedOil = await _unitOfWork.Oils.GetByVehiclePartIdAsync(oil.VehiclePartId);

                _logger.LogInformation("Updated oil with ID: {OilId}", id);

                return ApiResponse<OilResponse>.SuccessResponse(
                    updatedOil!.ToResponse(),
                    "Cập nhật thông tin nhớt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating oil with ID: {OilId}", id);
                return ApiResponse<OilResponse>.FailureResponse("Lỗi khi cập nhật thông tin nhớt");
            }
        }
    }
}
