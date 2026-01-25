using Microsoft.AspNetCore.Mvc;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Apis
{
    public static class VehicleVariantApis
    {
        public static IEndpointRouteBuilder MapModelImageApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/variants")
                .MapModelImageRoutes()
                .WithTags("Model Image Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapModelImageRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/model/{vehicleModelId:guid}", GetImagesByModelId)
                .WithName("GetImagesByModelId")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách hình ảnh/màu theo mẫu xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<VehicleVariantResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<VehicleVariantResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateModelImage)
                .WithName("CreateModelImage")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo hình ảnh/màu mới cho mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<VehicleVariantResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<VehicleVariantResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateModelImage)
                .WithName("UpdateModelImage")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật hình ảnh/màu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<VehicleVariantResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<VehicleVariantResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteModelImage)
                .WithName("DeleteModelImage")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa hình ảnh/màu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetImagesByModelId(
            Guid vehicleModelId,
            IVehicleVariantService modelImageService)
        {
            var result = await modelImageService.GetImagesByModelIdAsync(vehicleModelId);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> CreateModelImage(
            [FromBody] VehicleVariantRequest request,
            IVehicleVariantService modelImageService)
        {
            var result = await modelImageService.CreateImageAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateModelImage(
            Guid id,
            [FromBody] VehicleVariantUpdateRequest request,
            IVehicleVariantService modelImageService)
        {
            var result = await modelImageService.UpdateImageAsync(id, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteModelImage(
            Guid id,
            IVehicleVariantService modelImageService)
        {
            var result = await modelImageService.DeleteImageAsync(id);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }
    }
}
