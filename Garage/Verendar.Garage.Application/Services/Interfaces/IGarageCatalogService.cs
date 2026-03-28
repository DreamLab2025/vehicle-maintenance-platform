using Verendar.Common.Shared;
using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageCatalogService
{
    Task<ApiResponse<List<CatalogItemResponse>>> GetCatalogAsync(
        Guid branchId, CatalogQueryRequest query, CancellationToken ct = default);
}
