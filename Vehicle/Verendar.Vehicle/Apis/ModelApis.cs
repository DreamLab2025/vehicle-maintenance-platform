using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Apis
{
    public static class ModelApis
    {
        public static IEndpointRouteBuilder MapModelApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/models")
                .MapModelRoutes()
                .WithTags("Model Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapModelRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllModels)
                .WithName("GetAllModels")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách tất cả mẫu xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<ModelResponseWithVariants>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<ModelResponseWithVariants>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateVehicleModel)
                .WithName("CreateModel")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo mới mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelResponseWithVariants>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<ModelResponseWithVariants>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateVehicleModel)
                .WithName("UpdateModel")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteVehicleModel)
                .WithName("DeleteModel")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}", GetModelById)
                .WithName("GetModelById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin mẫu xe theo ID";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<ModelResponseWithVariants>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<ModelResponseWithVariants>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> DeleteVehicleModel(Guid id, IVehicleModelService modelService)
        {
            var result = await modelService.DeleteModelAsync(id);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateVehicleModel(Guid id, ModelRequest request, IVehicleModelService modelService)
        {
            var result = await modelService.UpdateModelAsync(id, request);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> CreateVehicleModel(ModelRequest request, IVehicleModelService modelService)
        {
            var result = await modelService.CreateModelAsync(request);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> GetAllModels([AsParameters] ModelFilterRequest filterRequest, IVehicleModelService modelService)
        {
            var results = await modelService.GetAllModelsAsync(filterRequest);
            if (results.IsSuccess)
            {
                return Results.Ok(results);
            }
            return Results.NotFound(results);
        }

        private static async Task<IResult> GetModelById(Guid id, IVehicleModelService modelService)
        {
            var result = await modelService.GetModelByIdAsync(id);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.NotFound(result);
        }

    }
}
