using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Services.Interfaces;

namespace VMP.Vehicle.Apis
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
                .Produces<ApiResponse<List<ModelResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<ModelResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateVehicleModel)
                .WithName("CreateModel")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo mới mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/bulk", BulkCreateModels)
                .WithName("BulkCreateModels")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo hàng loạt mẫu xe từ JSON body (Admin)";
                    operation.Description = "Gửi JSON body chứa BrandId, TypeId và danh sách models";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BulkModelResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BulkModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .DisableAntiforgery();

            group.MapPost("/bulk/upload", BulkCreateModelsFromFile)
                .WithName("BulkCreateModelsFromFile")
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BulkModelResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BulkModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .DisableAntiforgery();

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
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> BulkCreateModels(BulkModelRequest request, IVehicleModelService modelService)
        {
            var result = await modelService.BulkCreateModelsAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> BulkCreateModelsFromFile([FromForm] IFormFile file, IVehicleModelService modelService)
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(ApiResponse<BulkModelResponse>.FailureResponse("File không được để trống"));
            }

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(ApiResponse<BulkModelResponse>.FailureResponse("Chỉ chấp nhận file JSON"));
            }

            try
            {
                using var stream = file.OpenReadStream();
                var bulkRequest = await JsonSerializer.DeserializeAsync<BulkModelFileRequest>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (bulkRequest == null || bulkRequest.Models == null || !bulkRequest.Models.Any())
                {
                    return Results.BadRequest(ApiResponse<BulkModelResponse>.FailureResponse("File JSON không hợp lệ hoặc rỗng"));
                }

                var result = await modelService.BulkCreateModelsFromFileAsync(bulkRequest);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
            }
            catch (JsonException)
            {
                return Results.BadRequest(ApiResponse<BulkModelResponse>.FailureResponse("File JSON không đúng định dạng"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<BulkModelResponse>.FailureResponse($"Lỗi khi xử lý file: {ex.Message}"));
            }
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
