using Verendar.Media.Domain.Entities;

namespace Verendar.Media.Application.Dtos
{
    public class InitUploadRequest
    {
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Size { get; set; }
        public StorageProvider Provider { get; set; } = StorageProvider.AwsS3;
    }

    public class InitUploadResponse
    {
        public string PresignedUrl { get; set; } = null!;
        public string FileId { get; set; } = null!;
    }
}
