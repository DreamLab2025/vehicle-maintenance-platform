using Verendar.Common.EndpointFilters;
using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Media.Application.Dtos;
using Verendar.Media.Application.Services.Interfaces;

namespace Verendar.Media.Apis
{
    public static class MediaFileApis
    {
        public static IEndpointRouteBuilder MapMediaFileApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("api/v1/media-files")
                .WithTags("Media Files")
                .MapMediaFileRoutes()
                .RequireRateLimiting("Fixed")
                .RequireAuthorization();

            return builder;
        }

        public static RouteGroupBuilder MapMediaFileRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/init-upload", InitiateUpload)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<InitUploadRequest>())
                .WithName("InitUpload")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy Presigned URL để upload file lên S3";
                    operation.Description = "Truyền FileType để xác định loại file và thư mục lưu trữ trên S3 (Avatar, VehicleType, VehicleBrand, VehicleVariant, PartCategory, Other).";
                    return operation;
                })
                .Produces<ApiResponse<InitUploadResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InitUploadResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("{id:guid}/confirm", ConfirmUploadFile)
                .WithName("ConfirmUploadFile")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xác nhận upload file thành công";
                    return operation;
                })
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> InitiateUpload(
            InitUploadRequest request,
            ICurrentUserService currentUserService,
            IMediaUploadService mediaUploadService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Results.Unauthorized();

            var result = await mediaUploadService.InitiateUploadAsync(request, userId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> ConfirmUploadFile(
            Guid id,
            ICurrentUserService currentUserService,
            IMediaUploadService mediaUploadService)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Results.Unauthorized();

            var result = await mediaUploadService.ConfirmUploadFileAsync(id, userId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}
