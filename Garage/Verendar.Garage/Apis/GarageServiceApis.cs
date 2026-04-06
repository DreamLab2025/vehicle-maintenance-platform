using Microsoft.AspNetCore.Mvc;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class GarageServiceApis
{
    public static IEndpointRouteBuilder MapGarageServiceApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garage-services")
            .MapGarageServiceRoutes()
            .WithTags("Garage Service Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapGarageServiceRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetByBranch)
            .WithName("GetGarageServices")
            .WithOpenApi(op => { op.Summary = "Danh sách dịch vụ theo chi nhánh"; return op; })
            .Produces<ApiResponse<List<GarageServiceListItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<GarageServiceListItemResponse>>>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetGarageServiceById")
            .WithOpenApi(op => { op.Summary = "Chi tiết dịch vụ"; return op; })
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateGarageServiceRequest>())
            .WithName("CreateGarageService")
            .WithOpenApi(op => { op.Summary = "Owner/Manager tạo dịch vụ cho chi nhánh"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateGarageServiceRequest>())
            .WithName("UpdateGarageService")
            .WithOpenApi(op => { op.Summary = "Owner/Manager cập nhật dịch vụ"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/status", UpdateStatus)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateGarageServiceStatusRequest>())
            .WithName("UpdateGarageServiceStatus")
            .WithOpenApi(op => { op.Summary = "Owner/Manager bật/tắt trạng thái dịch vụ"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageServiceResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteGarageService")
            .WithOpenApi(op => { op.Summary = "Owner/Manager xóa dịch vụ (soft delete)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetByBranch(
        [AsParameters] GarageServiceQueryRequest query,
        IGarageServiceService serviceService,
        CancellationToken ct)
    {
        var result = await serviceService.GetServicesByBranchAsync(query, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id,
        IGarageServiceService serviceService,
        CancellationToken ct)
    {
        var result = await serviceService.GetServiceByIdAsync(id, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromQuery] Guid branchId,
        [FromBody] CreateGarageServiceRequest request,
        ICurrentUserService currentUserService,
        IGarageServiceService serviceService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await serviceService.CreateServiceAsync(branchId, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateGarageServiceRequest request,
        ICurrentUserService currentUserService,
        IGarageServiceService serviceService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await serviceService.UpdateServiceAsync(id, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateGarageServiceStatusRequest request,
        ICurrentUserService currentUserService,
        IGarageServiceService serviceService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await serviceService.UpdateServiceStatusAsync(id, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        ICurrentUserService currentUserService,
        IGarageServiceService serviceService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await serviceService.DeleteServiceAsync(id, userId, ct);
        return result.ToHttpResult();
    }
}
