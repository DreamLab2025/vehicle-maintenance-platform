using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Apis
{
    public static class TypeApis
    {
        public static IEndpointRouteBuilder MapTypeApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/types")
                .MapTypeRoutes()
                .WithTags("Type Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder MapTypeRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllTypes)
                .WithName("GetAllTypes")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách tất cả loại xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<TypeResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<TypeResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateVehicleType)
                .WithName("CreateType")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo mới loại xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateVehicleType)
                .WithName("UpdateType")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật loại xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteVehicleType)
                .WithName("DeleteType")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa loại xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> DeleteVehicleType(Guid id, IVehicleTypeService typeService)
        {
            var result = await typeService.DeleteTypeAsync(id);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateVehicleType(Guid id, TypeRequest request, IVehicleTypeService typeService)
        {
            var result = await typeService.UpdateTypeAsync(id, request);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> CreateVehicleType(TypeRequest request, IVehicleTypeService typeService)
        {
            var result = await typeService.CreateTypeAsync(request);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> GetAllTypes([AsParameters] PaginationRequest paginationRequest, IVehicleTypeService typeService)
        {
            var results = await typeService.GetAllTypesAsync(paginationRequest);
            if (results.IsSuccess)
            {
                return Results.Ok(results);
            }
            return Results.NotFound(results);
        }
    }
}
