using Microsoft.Extensions.Logging;

namespace Verendar.Ai.Infrastructure.ExternalServices
{
    internal static class GenerativeAiImageHttp
    {
        public const string UserAgent = "Verendar-Ai/1.0";

        public static string NormalizePublicImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return string.Empty;

            var t = imageUrl.Trim();
            if (t.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return t;

            return "https://" + t.TrimStart('/');
        }

        public static string TruncateUrlForLog(string? url, int maxLen = 160)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;
            return url.Length <= maxLen ? url : url[..maxLen] + "…";
        }

        public static async Task<ImageHttpFetchResult> FetchAsync(
            IHttpClientFactory httpClientFactory,
            string imageUrl,
            int timeoutSeconds,
            ILogger logger,
            string logCategory,
            CancellationToken cancellationToken)
        {
            var fetchUrl = NormalizePublicImageUrl(imageUrl);
            try
            {
                using var httpClient = httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);

                var imageResponse = await httpClient.GetAsync(fetchUrl, cancellationToken);
                if (!imageResponse.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "{Category}: failed to download image {Status} from {UrlPrefix}",
                        logCategory,
                        imageResponse.StatusCode,
                        TruncateUrlForLog(fetchUrl));
                    return ImageHttpFetchResult.DownloadFailed;
                }

                var bytes = await imageResponse.Content.ReadAsByteArrayAsync(cancellationToken);
                var mimeType = imageResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                return ImageHttpFetchResult.Succeeded(bytes, mimeType);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "{Category}: failed to download image from {ImageUrl}",
                    logCategory,
                    TruncateUrlForLog(NormalizePublicImageUrl(imageUrl)));
                return ImageHttpFetchResult.RequestFailed;
            }
        }
    }

    internal readonly struct ImageHttpFetchResult
    {
        public bool IsSuccess { get; private init; }
        public byte[]? Bytes { get; private init; }
        public string MimeType { get; private init; }
        public bool IsDownloadHttpFailure { get; private init; }
        public bool IsRequestException { get; private init; }

        public static ImageHttpFetchResult Succeeded(byte[] bytes, string mimeType) =>
            new()
            {
                IsSuccess = true,
                Bytes = bytes,
                MimeType = mimeType ?? "image/jpeg"
            };

        public static ImageHttpFetchResult DownloadFailed =>
            new() { IsDownloadHttpFailure = true };

        public static ImageHttpFetchResult RequestFailed =>
            new() { IsRequestException = true };
    }
}
