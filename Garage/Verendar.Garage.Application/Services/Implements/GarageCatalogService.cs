using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageCatalogService(
    ILogger<GarageCatalogService> logger,
    IUnitOfWork unitOfWork) : IGarageCatalogService
{
    private readonly ILogger<GarageCatalogService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // Giới hạn khi fetch toàn bộ items nội bộ để merge/filter.
    // Một chi nhánh garage điển hình sẽ không vượt quá ngưỡng này.
    private const int InternalFetchLimit = 500;

    public async Task<ApiResponse<List<CatalogItemResponse>>> GetCatalogAsync(
        Guid branchId, CatalogQueryRequest query, CancellationToken ct = default)
    {
        query.Normalize();

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.DeletedAt == null && b.Status == BranchStatus.Active);
        if (branch is null)
            return ApiResponse<List<CatalogItemResponse>>.NotFoundResponse(
                string.Format(EndpointMessages.Catalog.BranchNotFoundByIdFormat, branchId));

        var allItems = new List<CatalogItemResponse>();

        if (query.Type is null or CatalogItemType.Service)
        {
            var (services, _) = await _unitOfWork.GarageServices.GetPagedByBranchIdAsync(
                branchId, activeOnly: true, pageNumber: 1, pageSize: InternalFetchLimit, ct);

            var mapped = query.CategoryId.HasValue
                ? services
                    .Where(s => s.ServiceCategoryId == query.CategoryId)
                    .Select(s => s.ToCatalogItem())
                : services.Select(s => s.ToCatalogItem());

            allItems.AddRange(mapped);
        }

        if (query.Type is null or CatalogItemType.Product)
        {
            var (products, _) = await _unitOfWork.GarageProducts.GetPagedByBranchIdAsync(
                branchId, activeOnly: true, pageNumber: 1, pageSize: InternalFetchLimit, ct);
            allItems.AddRange(products.Select(p => p.ToCatalogItem()));
        }

        if (query.Type is null or CatalogItemType.Bundle)
        {
            var (bundles, _) = await _unitOfWork.GarageBundles.GetPagedByBranchIdAsync(
                branchId, activeOnly: true, pageNumber: 1, pageSize: InternalFetchLimit, ct);
            allItems.AddRange(bundles.Select(b => b.ToCatalogItem()));
        }

        allItems = [.. allItems.OrderBy(i => i.Name)];

        var totalCount = allItems.Count;
        var paged = allItems
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        _logger.LogDebug(
            "GetCatalog: branch={BranchId} type={Type} categoryId={CategoryId} total={Total}",
            branchId, query.Type, query.CategoryId, totalCount);

        return ApiResponse<List<CatalogItemResponse>>.SuccessPagedResponse(
            paged, totalCount, query.PageNumber, query.PageSize,
            EndpointMessages.Catalog.ListSuccess);
    }
}
