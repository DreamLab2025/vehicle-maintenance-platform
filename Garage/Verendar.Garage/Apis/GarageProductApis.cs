using Microsoft.AspNetCore.Mvc;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class GarageProductApis
{
    public static IEndpointRouteBuilder MapGarageProductApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garage-products")
            .MapGarageProductRoutes()
            .WithTags("Garage Product Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapGarageProductRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetByBranch)
            .WithName("GetGarageProducts")
            .WithOpenApi(op =>
            {
                op.Summary = "Danh sách sản phẩm/dịch vụ theo chi nhánh";
                return op;
            })
            .Produces<ApiResponse<List<GarageProductListItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<GarageProductListItemResponse>>>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetGarageProductById")
            .WithOpenApi(op =>
            {
                op.Summary = "Chi tiết sản phẩm (kèm items nếu là Bundle)";
                return op;
            })
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateGarageProductRequest>())
            .WithName("CreateGarageProduct")
            .WithOpenApi(op =>
            {
                op.Summary = "Owner/Manager tạo sản phẩm cho chi nhánh";
                return op;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateGarageProductRequest>())
            .WithName("UpdateGarageProduct")
            .WithOpenApi(op =>
            {
                op.Summary = "Owner/Manager cập nhật sản phẩm";
                return op;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/status", UpdateStatus)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateGarageProductStatusRequest>())
            .WithName("UpdateGarageProductStatus")
            .WithOpenApi(op =>
            {
                op.Summary = "Owner/Manager bật/tắt trạng thái sản phẩm";
                return op;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageProductResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteGarageProduct")
            .WithOpenApi(op =>
            {
                op.Summary = "Owner/Manager xóa sản phẩm (soft delete)";
                return op;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetByBranch(
        [AsParameters] GarageProductQueryRequest query,
        IGarageProductService productService,
        CancellationToken ct)
    {
        var result = await productService.GetProductsByBranchAsync(query, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id,
        IGarageProductService productService,
        CancellationToken ct)
    {
        var result = await productService.GetProductByIdAsync(id, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromQuery] Guid branchId,
        [FromBody] CreateGarageProductRequest request,
        ICurrentUserService currentUserService,
        IGarageProductService productService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await productService.CreateProductAsync(branchId, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateGarageProductRequest request,
        ICurrentUserService currentUserService,
        IGarageProductService productService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await productService.UpdateProductAsync(id, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateGarageProductStatusRequest request,
        ICurrentUserService currentUserService,
        IGarageProductService productService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await productService.UpdateProductStatusAsync(id, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        ICurrentUserService currentUserService,
        IGarageProductService productService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await productService.DeleteProductAsync(id, userId, ct);
        return result.ToHttpResult();
    }
}
