namespace VMP.Media.Infrastructure.Configuration
{
    public class S3Settings
    {
        public string BucketName { get; init; } = default!;
        public string Region { get; init; } = default!;
        public string AccessKey { get; init; } = default!;
        public string SecretKey { get; init; } = default!;
    }
}
