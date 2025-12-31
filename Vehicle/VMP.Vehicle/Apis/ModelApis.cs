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
                .WithSummary("Lấy danh sách tất cả mẫu xe")
                .WithDescription("Trả về danh sách tất cả mẫu xe trong hệ thống")
                .RequireAuthorization()
                .Produces<ApiResponse<List<ModelResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<ModelResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateVehicleModel)
                .WithName("CreateModel")
                .WithSummary("Tạo mới mẫu xe")
                .WithDescription("Tạo mới một mẫu xe trong hệ thống")
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/bulk", BulkCreateModels)
                .WithName("BulkCreateModels")
                .WithSummary("Tạo hàng loạt mẫu xe từ JSON")
                .WithDescription("Upload file JSON hoặc gửi JSON body để tạo nhiều mẫu xe cùng lúc")
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BulkModelResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BulkModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .DisableAntiforgery();

            group.MapPost("/bulk/upload", BulkCreateModelsFromFile)
                .WithName("BulkCreateModelsFromFile")
                .WithSummary("Tạo hàng loạt mẫu xe từ file JSON")
                .WithDescription("Upload file JSON để tạo nhiều mẫu xe cùng lúc")
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BulkModelResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BulkModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .DisableAntiforgery();

            group.MapPut("/{id:guid}", UpdateVehicleModel)
                .WithName("UpdateModel")
                .WithSummary("Cập nhật mẫu xe")
                .WithDescription("Cập nhật thông tin một mẫu xe trong hệ thống")
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteVehicleModel)
                .WithName("DeleteModel")
                .WithSummary("Xóa mẫu xe")
                .WithDescription("Xóa một mẫu xe khỏi hệ thống")
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> BulkCreateModels(BulkModelRequest request, IVehicleModelService modelService)
        {
            var result = await modelService.BulkCreateModelsAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> BulkCreateModelsFromFile(IFormFile file, IVehicleModelService modelService)
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
                var bulkRequest = await JsonSerializer.DeserializeAsync<BulkModelRequest>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (bulkRequest == null || bulkRequest.Models == null || !bulkRequest.Models.Any())
                {
                    return Results.BadRequest(ApiResponse<BulkModelResponse>.FailureResponse("File JSON không hợp lệ hoặc rỗng"));
                }

                var result = await modelService.BulkCreateModelsAsync(bulkRequest);
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

        private static async Task<IResult> GetAllModels([AsParameters] PaginationRequest paginationRequest, IVehicleModelService modelService)
        {
            var results = await modelService.GetAllModelsAsync(paginationRequest);
            if (results.IsSuccess)
            {
                return Results.Ok(results);
            }
            return Results.NotFound(results);
        }
    }
}
