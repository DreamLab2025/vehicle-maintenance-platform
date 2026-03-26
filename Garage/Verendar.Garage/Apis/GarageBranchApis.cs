using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class GarageBranchApis
{
    public static IEndpointRouteBuilder MapGarageBranchApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garages/{garageId:guid}/branches")
            .MapGarageBranchRoutes()
            .WithTags("Garage Branch Api")
            .RequireRateLimiting("Fixed");

        builder.MapGroup("/api/v1/garages/branches")
            .MapGarageBranchMapRoutes()
            .WithTags("Garage Branch Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapGarageBranchRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetBranches)
            .WithName("GetGarageBranches")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách chi nhánh của garage (có phân trang)";
                return operation;
            })
            .Produces<ApiResponse<List<GarageBranchSummaryResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<GarageBranchSummaryResponse>>>(StatusCodes.Status404NotFound);

        group.MapGet("/{branchId:guid}", GetBranchById)
            .WithName("GetGarageBranchById")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Xem chi tiết chi nhánh garage";
                return operation;
            })
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBranch)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<GarageBranchRequest>())
            .WithName("CreateGarageBranch")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tạo chi nhánh mới cho garage (chỉ chủ garage)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/{branchId:guid}", UpdateBranch)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<GarageBranchRequest>())
            .WithName("UpdateGarageBranch")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Cập nhật chi nhánh garage (chỉ chủ garage)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{branchId:guid}", DeleteBranch)
            .WithName("DeleteGarageBranch")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Xóa chi nhánh garage (chỉ chủ garage, soft delete)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{branchId:guid}/status", UpdateBranchStatus)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateBranchStatusRequest>())
            .WithName("UpdateGarageBranchStatus")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Chủ garage bật/tắt hoạt động chi nhánh (Active ↔ Inactive)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    public static RouteGroupBuilder MapGarageBranchMapRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/maps", SearchBranchesOnMap)
            .WithName("SearchGarageBranchesOnMap")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tìm kiếm chi nhánh trên bản đồ (theo địa chỉ hoặc tọa độ + bán kính)";
                return operation;
            })
            .Produces<ApiResponse<List<BranchMapItemResponse>>>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> GetBranches(
        Guid garageId,
        [AsParameters] PaginationRequest request,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.GetBranchesAsync(garageId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateBranch(
        Guid garageId,
        Guid branchId,
        GarageBranchRequest request,
        ICurrentUserService currentUserService,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.UpdateBranchAsync(garageId, branchId, currentUserService.UserId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteBranch(
        Guid garageId,
        Guid branchId,
        ICurrentUserService currentUserService,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.DeleteBranchAsync(garageId, branchId, currentUserService.UserId, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateBranchStatus(
        Guid garageId,
        Guid branchId,
        UpdateBranchStatusRequest request,
        ICurrentUserService currentUserService,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.UpdateBranchStatusAsync(garageId, branchId, currentUserService.UserId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> SearchBranchesOnMap(
        [AsParameters] BranchMapSearchRequest request,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.GetBranchesForMapAsync(request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetBranchById(
        Guid garageId,
        Guid branchId,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.GetBranchByIdAsync(garageId, branchId, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateBranch(
        Guid garageId,
        GarageBranchRequest request,
        ICurrentUserService currentUserService,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.CreateBranchAsync(
            garageId,
            currentUserService.UserId,
            request,
            ct);
        return result.ToHttpResult();
    }
}
