using VMP.Common.Jwt;
using VMP.Common.Shared;
using VMP.Media.Application.Dtos;
using VMP.Media.Application.Services.Interfaces;

namespace VMP.Media.Apis
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
                .WithName("Initiate Upload")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy Presigned URL để upload file";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<InitUploadResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InitUploadResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("{id}/confirm", ConfirmUploadFile)
                .WithName("Confirm Upload File")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xác nhận upload file thành công";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> ConfirmUploadFile(
            Guid id,
            ICurrentUserService currentUserService,
            IMediaUploadService mediaUploadService)
        {
            var result = await mediaUploadService.ConfirmUploadFileAsync(id, currentUserService.UserId);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }

        private static async Task<IResult> InitiateUpload(
            InitUploadRequest request,
            IMediaUploadService mediaUploadService,
            ICurrentUserService currentUser)
        {
            var result = await mediaUploadService.InitiateUploadAsync(request, currentUser.UserId);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }
    }
}
