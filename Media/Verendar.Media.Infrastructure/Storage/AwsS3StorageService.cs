using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System.Net;
using Verendar.Media.Application.IStorage;
using Verendar.Media.Infrastructure.Configuration;

namespace Verendar.Media.Infrastructure.Storage
{
    public class AwsS3StorageService(IAmazonS3 s3Client, IOptions<S3Settings> s3Settings) : IStorageService
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly S3Settings _s3settings = s3Settings.Value;

        public async Task DeleteFileAsync(string fileKey)
        {
            if (string.IsNullOrWhiteSpace(fileKey))
            {
                return;
            }

            try
            {
                await _s3Client.DeleteObjectAsync(_s3settings.BucketName, fileKey);
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }

        public string ExtractKeyFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            try
            {
                var uri = new Uri(url);

                // Handle CloudFront URL
                if (!string.IsNullOrWhiteSpace(_s3settings.CloudFrontUrl) &&
                    url.StartsWith(_s3settings.CloudFrontUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return WebUtility.UrlDecode(uri.AbsolutePath.TrimStart('/'));
                }

                // Handle S3 URL
                return WebUtility.UrlDecode(uri.AbsolutePath.TrimStart('/'));
            }
            catch
            {
                return null;
            }
        }

        //public static string? ExtractFirebasePath(string fullUrl)
        //{
        //    if (string.IsNullOrWhiteSpace(fullUrl)) return null;

        //    try
        //    {
        //        var parts = fullUrl.Split(new[] { "/o/" }, StringSplitOptions.None);

        //        if (parts.Length < 2) return null;

        //        var segment = parts[1];

        //        var queryIndex = segment.IndexOf('?');
        //        if (queryIndex != -1)
        //        {
        //            segment = segment.Substring(0, queryIndex);
        //        }

        //        return WebUtility.UrlDecode(segment);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        public async Task<string> GeneratePresignedUrlAsync(string fileKey, string contentType)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3settings.BucketName,
                Key = fileKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(5),
                ContentType = contentType
            };

            request.Headers["Content-Type"] = contentType;

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }

        public string GetFilePath(string fileKey)
        {
            // Use CloudFront URL if configured, otherwise use S3 URL
            if (!string.IsNullOrWhiteSpace(_s3settings.CloudFrontUrl))
            {
                return $"{_s3settings.CloudFrontUrl.TrimEnd('/')}/{fileKey}";
            }

            return $"https://{_s3settings.BucketName}.s3.{_s3settings.Region}.amazonaws.com/{fileKey}";
        }
    }
}
