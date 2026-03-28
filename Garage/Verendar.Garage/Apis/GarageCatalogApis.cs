using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class GarageCatalogApis
{
    public static IEndpointRouteBuilder MapGarageCatalogApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garage-catalog")
            .MapGarageCatalogRoutes()
            .WithTags("Garage Catalog Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapGarageCatalogRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/{branchId:guid}", GetCatalog)
            .WithName("GetGarageCatalog")
            .WithOpenApi(op =>
            {
                op.Summary = "Xem toàn bộ danh mục (dịch vụ, sản phẩm, combo) của một chi nhánh";
                op.Description = "Public endpoint. Lọc theo type (service/product/bundle) và/hoặc categoryId. " +
                                 "Chỉ trả về items Active của chi nhánh đang Active.";
                return op;
            })
            .Produces<ApiResponse<List<CatalogItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CatalogItemResponse>>>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetCatalog(
        Guid branchId,
        [AsParameters] CatalogQueryRequest query,
        IGarageCatalogService catalogService,
        CancellationToken ct)
    {
        var result = await catalogService.GetCatalogAsync(branchId, query, ct);
        return result.ToHttpResult();
    }
}
