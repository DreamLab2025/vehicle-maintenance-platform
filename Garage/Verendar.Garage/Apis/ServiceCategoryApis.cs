using Microsoft.AspNetCore.Mvc;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class ServiceCategoryApis
{
    public static IEndpointRouteBuilder MapServiceCategoryApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/service-categories")
            .MapServiceCategoryRoutes()
            .WithTags("Service Category Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapServiceCategoryRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll)
            .WithName("GetServiceCategories")
            .WithOpenApi(op =>
            {
                op.Summary = "Danh sách danh mục dịch vụ (rửa xe, kiểm tra tổng quát…)";
                return op;
            })
            .Produces<ApiResponse<List<ServiceCategoryResponse>>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetServiceCategoryById")
            .WithOpenApi(op =>
            {
                op.Summary = "Chi tiết danh mục dịch vụ";
                return op;
            })
            .Produces<ApiResponse<ServiceCategoryResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ServiceCategoryResponse>>(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateServiceCategoryRequest>())
            .WithName("CreateServiceCategory")
            .WithOpenApi(op =>
            {
                op.Summary = "Tạo danh mục dịch vụ (Admin only)";
                return op;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<ServiceCategoryResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<ServiceCategoryResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ServiceCategoryResponse>>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateServiceCategoryRequest>())
            .WithName("UpdateServiceCategory")
            .WithOpenApi(op =>
            {
                op.Summary = "Cập nhật danh mục dịch vụ (Admin only)";
                return op;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<ServiceCategoryResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ServiceCategoryResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ServiceCategoryResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteServiceCategory")
            .WithOpenApi(op =>
            {
                op.Summary = "Xóa danh mục dịch vụ (Admin only)";
                return op;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetAll(
        IServiceCategoryService service,
        CancellationToken ct)
    {
        var result = await service.GetAllAsync(ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id,
        IServiceCategoryService service,
        CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromBody] CreateServiceCategoryRequest request,
        IServiceCategoryService service,
        CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceCategoryRequest request,
        IServiceCategoryService service,
        CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        IServiceCategoryService service,
        CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.ToHttpResult();
    }
}
