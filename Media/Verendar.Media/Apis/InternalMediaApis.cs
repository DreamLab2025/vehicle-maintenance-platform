namespace Verendar.Media.Apis
{
    public static class InternalMediaApis
    {
        public static IEndpointRouteBuilder MapInternalMediaApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/internal/media-files")
                .WithTags("Internal Media")
                .MapInternalMediaRoutes();

            return builder;
        }

        private static RouteGroupBuilder MapInternalMediaRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/{id:guid}/url", GetMediaFileUrl)
                .WithName("InternalGetMediaFileUrl")
                .ExcludeFromDescription();

            return group;
        }

        private static async Task<IResult> GetMediaFileUrl(
            Guid id,
            IMediaUploadService mediaUploadService,
            CancellationToken cancellationToken)
        {
            var result = await mediaUploadService.GetMediaFileUrlAsync(id, cancellationToken);
            return result.ToHttpResult();
        }
    }
}
