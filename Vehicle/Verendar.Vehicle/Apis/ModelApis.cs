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
                .AddEndpointFilter(ValidationEndpointFilter.Validate<ModelFilterRequest>())
                .WithName("GetAllModels")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách tất cả mẫu xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<ModelSummary>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateVehicleModel)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<ModelRequest>())
                .WithName("CreateModel")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo mới mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelResponseWithVariants>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<ModelResponseWithVariants>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<ModelResponseWithVariants>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<ModelResponseWithVariants>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateVehicleModel)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<ModelRequest>())
                .WithName("UpdateModel")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<ModelResponse>>(StatusCodes.Status409Conflict)
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
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
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

            group.MapGet("/{id:guid}/variants", GetVariantsByModelId)
                .WithName("GetVariantsByModelId")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách hình ảnh/màu theo mẫu xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<VariantResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<VariantResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> DeleteVehicleModel(Guid id, IModelService modelService)
        {
            var result = await modelService.DeleteModelAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateVehicleModel(Guid id, ModelRequest request, IModelService modelService)
        {
            var result = await modelService.UpdateModelAsync(id, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> CreateVehicleModel(ModelRequest request, IModelService modelService)
        {
            var result = await modelService.CreateModelAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetAllModels([AsParameters] ModelFilterRequest filterRequest, IModelService modelService)
        {
            var results = await modelService.GetAllModelsAsync(filterRequest);
            return results.ToHttpResult();
        }

        private static async Task<IResult> GetModelById(Guid id, IModelService modelService)
        {
            var result = await modelService.GetModelByIdAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetVariantsByModelId(Guid id, IVariantService variantService)
        {
            var result = await variantService.GetImagesByModelIdAsync(id);
            return result.ToHttpResult();
        }
    }
}
