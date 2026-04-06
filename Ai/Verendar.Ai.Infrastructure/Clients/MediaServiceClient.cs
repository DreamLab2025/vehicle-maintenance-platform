using Verendar.Ai.Application.Clients;
using Verendar.Common.Http;

namespace Verendar.Ai.Infrastructure.Clients
{
    public class MediaServiceClient(HttpClient httpClient, ILogger<MediaServiceClient> logger)
        : BaseServiceClient<MediaServiceClient>(httpClient, logger), IMediaServiceClient
    {
        protected override string ServiceName => "Media Service";

        public async Task<string?> GetMediaFileUrlAsync(Guid mediaFileId, CancellationToken cancellationToken = default)
        {
            var result = await GetAsync<string>(
                $"/api/internal/media-files/{mediaFileId}/url",
                $"media file URL for {mediaFileId}",
                cancellationToken);

            return result.IsSuccess ? result.Data : null;
        }
    }
}
