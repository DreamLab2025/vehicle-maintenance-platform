using Microsoft.AspNetCore.Mvc;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Services.Interfaces;

namespace VMP.Vehicle.Apis
{
    public static class ModelImageApis
    {
        public static IEndpointRouteBuilder MapModelImageApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/model-images")
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
                .Produces<ApiResponse<List<ModelImageResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<ModelImageResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateModelImage)
                .WithName("CreateModelImage")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo hình ảnh/màu mới cho mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelImageResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<ModelImageResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateModelImage)
                .WithName("UpdateModelImage")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật hình ảnh/màu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelImageResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<ModelImageResponse>>(StatusCodes.Status400BadRequest)
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
            IModelImageService modelImageService)
        {
            var result = await modelImageService.GetImagesByModelIdAsync(vehicleModelId);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> CreateModelImage(
            [FromBody] ModelImageRequest request,
            IModelImageService modelImageService)
        {
            var result = await modelImageService.CreateImageAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateModelImage(
            Guid id,
            [FromBody] ModelImageUpdateRequest request,
            IModelImageService modelImageService)
        {
            var result = await modelImageService.UpdateImageAsync(id, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteModelImage(
            Guid id,
            IModelImageService modelImageService)
        {
            var result = await modelImageService.DeleteImageAsync(id);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }
    }
}
