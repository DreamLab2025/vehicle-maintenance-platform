using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using VMP.Media.Application.IStorage;
using VMP.Media.Infrastructure.Configuration;

namespace VMP.Media.Infrastructure.Storage
{
    public class AwsS3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3Settings _s3settings;

        public AwsS3StorageService(IAmazonS3 s3Client, IOptions<S3Settings> s3Settings)
        {
            _s3Client = s3Client;
            _s3settings = s3Settings.Value;
        }

        public Task DeleteFileAsync(string fileKey)
        {
            return _s3Client.DeleteObjectAsync(_s3settings.BucketName, fileKey);
        }

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
            return $"https://{_s3settings.BucketName}.s3.{_s3settings.Region}.amazonaws.com/{fileKey}";
        }
    }
}
