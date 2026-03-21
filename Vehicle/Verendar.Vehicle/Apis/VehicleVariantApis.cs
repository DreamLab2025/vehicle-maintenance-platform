using Microsoft.AspNetCore.Mvc;

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
            group.MapPost("/", CreateModelImage)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<VariantRequest>())
                .WithName("CreateModelImage")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo hình ảnh/màu mới cho mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateModelImage)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<VariantUpdateRequest>())
                .WithName("UpdateModelImage")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật hình ảnh/màu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status409Conflict)
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

        private static async Task<IResult> CreateModelImage([FromBody] VariantRequest request, IVariantService modelImageService)
        {
            var result = await modelImageService.CreateImageAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateModelImage(Guid id, [FromBody] VariantUpdateRequest request, IVariantService modelImageService)
        {
            var result = await modelImageService.UpdateImageAsync(id, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteModelImage(Guid id, IVariantService modelImageService)
        {
            var result = await modelImageService.DeleteImageAsync(id);
            return result.ToHttpResult();
        }
    }
}
